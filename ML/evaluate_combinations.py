"""Run the GoEmotions model on every combination of control-question answers.

Reads test_answers.json, writes results/combination_report.md.
Run from the ML folder:  .venv/Scripts/python.exe evaluate_combinations.py
"""
import itertools
import json
from collections import defaultdict
from pathlib import Path

import numpy as np
import onnxruntime as ort
from tokenizers import Tokenizer

HERE = Path(__file__).parent
MODEL_DIR = HERE / "models" / "goemotions"

# Google's official GoEmotions grouping (goemotions/data/sentiment_mapping.json), plus neutral
GROUPS = {
    "positive": ["amusement", "excitement", "joy", "love", "desire", "optimism", "caring",
                 "pride", "admiration", "gratitude", "relief", "approval"],
    "negative": ["fear", "nervousness", "remorse", "embarrassment", "disappointment",
                 "sadness", "grief", "disgust", "anger", "annoyance", "disapproval"],
    "ambiguous": ["realization", "surprise", "curiosity", "confusion"],
    "neutral": ["neutral"],
}
GROUP_OF = {label: group for group, labels in GROUPS.items() for label in labels}
KINDS = ["positive", "neutral", "negative"]
SHORT = {"positive": "P", "neutral": "0", "negative": "N"}


class GoEmotionsModel:
    def __init__(self, model_dir):
        config = json.loads((model_dir / "config.json").read_text(encoding="utf-8"))
        self.labels = [config["id2label"][str(i)] for i in range(len(config["id2label"]))]
        self.tokenizer = Tokenizer.from_file(str(model_dir / "tokenizer.json"))
        self.tokenizer.enable_truncation(max_length=512)
        self.session = ort.InferenceSession(str(model_dir / "model.onnx"),
                                            providers=["CPUExecutionProvider"])

    def scores(self, text):
        """Returns {label: score 0..1}. Multi-label model, so each label gets its own sigmoid."""
        encoding = self.tokenizer.encode(text)
        ids = np.array([encoding.ids], dtype=np.int64)
        mask = np.array([encoding.attention_mask], dtype=np.int64)
        logits = self.session.run(["logits"], {"input_ids": ids, "attention_mask": mask})[0][0]
        probabilities = 1.0 / (1.0 + np.exp(-logits))
        return dict(zip(self.labels, probabilities.tolist()))


def summarize(scores):
    ranked = sorted(scores.items(), key=lambda item: item[1], reverse=True)
    top_label, top_score = ranked[0]
    group_sums = defaultdict(float)
    group_max = defaultdict(float)
    for label, score in scores.items():
        group_sums[GROUP_OF[label]] += score
        group_max[GROUP_OF[label]] = max(group_max[GROUP_OF[label]], score)
    winning_group = max(group_sums, key=group_sums.get)
    winning_group_by_max = max(group_max, key=group_max.get)
    return {
        "top3": ranked[:3],
        "top_label": top_label,
        "top_score": top_score,
        "group_sums": dict(group_sums),
        "winning_group": winning_group,
        "winning_group_by_max": winning_group_by_max,
        "agree": GROUP_OF[top_label] == winning_group,
    }


def fmt_top3(top3):
    return ", ".join(f"{label} {score:.2f}" for label, score in top3)


def fmt_groups(sums):
    return " / ".join(f"{group[:3]} {sums.get(group, 0):.2f}" for group in GROUPS)


def main():
    data = json.loads((HERE / "test_answers.json").read_text(encoding="utf-8"))
    questions = data["questions"]
    model = GoEmotionsModel(MODEL_DIR)
    lines = ["# Combination Report: GoEmotions (SamLowe/roberta-base-go_emotions-onnx, full precision)", "",
             "Answers to the 3 control questions are joined with a space and classified as one text.",
             "Scores are per-label sigmoid probabilities (multi-label). Group = sum of member-label scores,",
             "using Google's GoEmotions sentiment mapping. **DISAGREE** = the top emotion is not in the winning group.",
             "Combination codes: P = strong positive, 0 = neutral, N = strong negative, in question order (q022, q013, q031).",
             ""]

    lines += ["## Each answer on its own", "", "| Question | Kind | Answer | Top 3 | Winning group | Check |",
              "|---|---|---|---|---|---|"]
    for question in questions:
        for kind in KINDS:
            text = question["answers"][kind]
            result = summarize(model.scores(text))
            lines.append(f"| {question['questionId']} | {kind} | {text} | {fmt_top3(result['top3'])} | "
                         f"{result['winning_group']} | {'ok' if result['agree'] else '**DISAGREE**'} |")

    lines += ["", "## All 27 orderings", "",
              "| Combo | Top 3 | Group sums (pos/neg/amb/neu) | Winning group (sum) | Winning group (max) | Check |",
              "|---|---|---|---|---|---|"]
    by_multiset = defaultdict(list)
    for combo in itertools.product(KINDS, repeat=3):
        text = " ".join(q["answers"][kind] for q, kind in zip(questions, combo))
        result = summarize(model.scores(text))
        code = "".join(SHORT[kind] for kind in combo)
        by_multiset["".join(sorted(code))].append((code, result))
        lines.append(f"| {code} | {fmt_top3(result['top3'])} | {fmt_groups(result['group_sums'])} | "
                     f"{result['winning_group']} | {result['winning_group_by_max']} | "
                     f"{'ok' if result['agree'] else '**DISAGREE**'} |")

    lines += ["", "## Does order matter? (10 distinct mixes)", "",
              "| Mix | Orderings | Top emotions seen | Winning groups seen | Top-score range |", "|---|---|---|---|---|"]
    for mix, results in sorted(by_multiset.items()):
        tops = sorted({r["top_label"] for _, r in results})
        groups = sorted({r["winning_group"] for _, r in results})
        scores = [r["top_score"] for _, r in results]
        lines.append(f"| {mix} | {len(results)} | {', '.join(tops)} | {', '.join(groups)} | "
                     f"{min(scores):.2f}–{max(scores):.2f} |")

    out = HERE / "results" / "combination_report.md"
    out.parent.mkdir(exist_ok=True)
    out.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {out}")


if __name__ == "__main__":
    main()
