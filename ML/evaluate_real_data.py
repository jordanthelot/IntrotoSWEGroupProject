"""Measure single-answer accuracy on real labeled text: the GoEmotions test split (Reddit comments).

Each comment's true valence comes from its human labels via Google's sentiment mapping:
  any negative label -> negative, else any positive label -> positive, else neutral
  (comments labeled with BOTH positive and negative emotions are reported separately as "mixed").
Predictions use the per-answer rule chosen in compare_strategies.py (group scoring: mean),
with sum and top1 shown for comparison.

Run from the ML folder:  .venv/Scripts/python.exe evaluate_real_data.py
"""
import json
from collections import Counter
from pathlib import Path

import numpy as np

from compare_strategies import VALENCE, classify
from evaluate_combinations import GROUP_OF, MODEL_DIR, GoEmotionsModel

HERE = Path(__file__).parent
DATA = HERE / "data" / "goemotions"
CACHE = DATA / "test_scores.npy"
OUTCOMES = ["positive", "neutral", "negative"]


def true_valence(label_names):
    valences = {VALENCE[GROUP_OF[name]] for name in label_names}
    if "positive" in valences and "negative" in valences:
        return "mixed"
    if "negative" in valences:
        return "negative"
    return "positive" if "positive" in valences else "neutral"


def main():
    emotions = (DATA / "emotions.txt").read_text(encoding="utf-8").split()
    rows = []
    for line in (DATA / "test.tsv").read_text(encoding="utf-8").splitlines():
        text, label_ids, _ = line.split("\t")
        rows.append((text, [emotions[int(i)] for i in label_ids.split(",")]))

    model = GoEmotionsModel(MODEL_DIR)
    if CACHE.exists():
        matrix = np.load(CACHE)
    else:
        matrix = np.zeros((len(rows), len(model.labels)), dtype=np.float32)
        for i, (text, _) in enumerate(rows):
            scores = model.scores(text)
            matrix[i] = [scores[label] for label in model.labels]
            if i % 500 == 0:
                print(f"scored {i}/{len(rows)}")
        np.save(CACHE, matrix)

    truths = [true_valence(names) for _, names in rows]
    lines = ["# Real-Data Test: single-answer valence accuracy on GoEmotions (test split)", "",
             f"{len(rows)} real Reddit comments with human emotion labels, held out from the model's training.",
             f"True valence counts: {dict(Counter(truths))}. Mixed comments (positive AND negative labels) "
             "are excluded from accuracy and listed separately.", "",
             "**Harmful rate** = share of truly negative comments read as positive (the error that matters most).",
             "**Negative recall** = share of truly negative comments read as negative.", ""]

    summary = {}
    for rule in ("mean", "sum", "top1"):
        predictions = [classify(dict(zip(model.labels, row.tolist())), rule, False) for row in matrix]
        confusion = Counter((t, p) for t, p in zip(truths, predictions) if t != "mixed")
        scored = sum(confusion.values())
        correct = sum(confusion[(o, o)] for o in OUTCOMES)
        negatives = sum(confusion[("negative", p)] for p in OUTCOMES)
        harmful = confusion[("negative", "positive")]
        mixed_predictions = Counter(p for t, p in zip(truths, predictions) if t == "mixed")
        summary[rule] = (correct / scored, confusion[("negative", "negative")] / negatives, harmful / negatives)
        lines += [f"## Group scoring: {rule}", "",
                  f"- Accuracy: **{correct / scored:.1%}** ({correct}/{scored})",
                  f"- Negative recall: **{confusion[('negative', 'negative')] / negatives:.1%}**",
                  f"- Harmful rate: **{harmful / negatives:.1%}** ({harmful} of {negatives} negative comments read as positive)",
                  f"- Mixed comments were read as: {dict(mixed_predictions)}", "",
                  "| True \\ Predicted | positive | neutral | negative |", "|---|---|---|---|"]
        for t in OUTCOMES:
            lines.append(f"| {t} | " + " | ".join(str(confusion[(t, p)]) for p in OUTCOMES) + " |")
        lines.append("")
        if rule == "mean":
            misses = [(text, names) for (text, names), t, p in zip(rows, truths, predictions)
                      if t == "negative" and p == "positive"]
            lines += ["**Examples of harmful misses (mean scoring), first 10:**", ""]
            for text, names in misses[:10]:
                short = text if len(text) <= 120 else text[:117] + "..."
                lines.append(f"- \"{short}\" (human labels: {', '.join(names)})")
            lines.append("")

    lines += ["## Caveats", "",
              "- These are Reddit comments, the same kind of text the model was trained on, so real player answers "
              "(tweet-like, about social media habits) may score lower.",
              "- Human labels are themselves noisy: GoEmotions raters often disagreed.",
              "- Data: GoEmotions by Google Research (Apache 2.0)."]
    out = HERE / "results" / "real_data_report.md"
    out.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {out}")
    for rule, (accuracy, recall, harmful) in summary.items():
        print(f"{rule:5} accuracy {accuracy:.1%}  negative recall {recall:.1%}  harmful {harmful:.1%}")


if __name__ == "__main__":
    main()
