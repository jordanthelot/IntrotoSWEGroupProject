"""Export the GoEmotions model, tokenizer data and test fixtures for Unity.

Writes:
  Assets/Resources/ReflectionML/goemotions.onnx          (the model, tracked with Git LFS)
  Assets/Resources/ReflectionML/goemotions_vocab.txt     (one token per line; line number = token id)
  Assets/Resources/ReflectionML/goemotions_merges.txt    (BPE merge rules, highest priority first)
  Assets/Resources/ReflectionML/goemotions_labels.txt    (the 28 emotion labels in output order)
  Assets/Tests/EditMode/ReflectionML/ml_fixtures.json    (expected token ids, scores and outcomes for the C# tests)

Run from the ML folder:  .venv/Scripts/python.exe export_unity_assets.py
"""
import itertools
import json
import shutil
from pathlib import Path

from compare_strategies import classify, combine
from evaluate_combinations import KINDS, MODEL_DIR, SHORT, GoEmotionsModel

HERE = Path(__file__).parent
REPO = HERE.parent
RESOURCES = REPO / "Assets" / "Resources" / "ReflectionML"
FIXTURES = REPO / "Assets" / "Tests" / "EditMode" / "ReflectionML" / "ml_fixtures.json"
RULE = "sum"  # group scoring chosen after the real-data test


def write_lines(path, lines):
    for line in lines:
        assert "\n" not in line and "\r" not in line, repr(line)
    path.write_bytes(("\n".join(lines) + "\n").encode("utf-8"))


def main():
    RESOURCES.mkdir(parents=True, exist_ok=True)
    tokenizer_json = json.loads((MODEL_DIR / "tokenizer.json").read_text(encoding="utf-8"))
    vocab = tokenizer_json["model"]["vocab"]
    by_id = sorted(vocab.items(), key=lambda item: item[1])
    assert [token_id for _, token_id in by_id] == list(range(len(by_id))), "vocab ids must be contiguous"
    write_lines(RESOURCES / "goemotions_vocab.txt", [token for token, _ in by_id])
    write_lines(RESOURCES / "goemotions_merges.txt", tokenizer_json["model"]["merges"])

    model = GoEmotionsModel(MODEL_DIR)
    model.tokenizer.no_padding()  # Unity sends unpadded input; the attention mask makes padding irrelevant
    write_lines(RESOURCES / "goemotions_labels.txt", model.labels)
    shutil.copyfile(MODEL_DIR / "model.onnx", RESOURCES / "goemotions.onnx")

    data = json.loads((HERE / "test_answers.json").read_text(encoding="utf-8"))
    questions = data["questions"]
    answers, outcome_of = [], {}
    extra_texts = ["", "   ", "I'm NOT okay... 😞 #tired", "café naïve — “quotes” & numbers 123 4.5"]
    for question in questions:
        for kind in KINDS:
            text = question["answers"][kind]
            scores = model.scores(text)
            outcome_of[(question["questionId"], kind)] = classify(scores, RULE, False)
            answers.append({"questionId": question["questionId"], "kind": kind, "text": text,
                            "tokenIds": model.tokenizer.encode(text).ids,
                            "scores": [scores[label] for label in model.labels],
                            "expectedValence": outcome_of[(question["questionId"], kind)]})
    tokenizer_cases = [{"text": text, "tokenIds": model.tokenizer.encode(text).ids} for text in extra_texts]

    combos = []
    for combo in itertools.product(KINDS, repeat=3):
        valences = [outcome_of[(q["questionId"], k)] for q, k in zip(questions, combo)]
        combos.append({"code": "".join(SHORT[k] for k in combo), "kinds": list(combo),
                       "answerValences": valences, "expectedOverall": combine(valences, "any-negative")})

    FIXTURES.parent.mkdir(parents=True, exist_ok=True)
    FIXTURES.write_text(json.dumps({"groupScoring": RULE, "labels": model.labels, "answers": answers,
                                    "tokenizerCases": tokenizer_cases, "combos": combos},
                                   ensure_ascii=False, indent=1), encoding="utf-8")
    print("Exported to", RESOURCES, "and", FIXTURES)
    for combo in combos:
        print(combo["code"], combo["answerValences"], "->", combo["expectedOverall"])


if __name__ == "__main__":
    main()
