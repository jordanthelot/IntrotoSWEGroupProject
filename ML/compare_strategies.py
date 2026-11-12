"""Compare ways of turning GoEmotions scores into one outcome for the player's 3 answers.

Strategies:
  Joined text (3 answers classified as one text), with different group-scoring rules:
    sum, mean, top-k (only the k highest labels in each group count; top-1 = max)
  Per answer (each answer classified alone, then combined):
    majority, any-negative, average-scores
  Each can also use the umbrella check: if the top emotion isn't in the winning group -> safe bag.

Error definitions (proposed, see report):
  ideal outcome = negative if any answer is strongly negative, else positive if any is positive, else neutral
  harmful       = outcome is positive although at least one answer is strongly negative
  total errors  = outcome != ideal (a safe-bag outcome is a miss, never harmful)

Run from the ML folder:  .venv/Scripts/python.exe compare_strategies.py
"""
import itertools
import json
from pathlib import Path

from evaluate_combinations import GROUPS, GROUP_OF, KINDS, MODEL_DIR, SHORT, GoEmotionsModel

HERE = Path(__file__).parent
VALENCE = {"positive": "positive", "negative": "negative", "ambiguous": "neutral", "neutral": "neutral"}


def group_scores(scores, rule):
    result = {}
    for group, labels in GROUPS.items():
        values = sorted((scores[label] for label in labels), reverse=True)
        if rule == "sum":
            result[group] = sum(values)
        elif rule == "mean":
            result[group] = sum(values) / len(values)
        else:  # "top1", "top2", ...: count only the k strongest labels in the group
            result[group] = sum(values[:int(rule[3:])])
    return result


def classify(scores, rule, umbrella):
    groups = group_scores(scores, rule)
    winner = max(groups, key=groups.get)
    if umbrella:
        top_label = max(scores, key=scores.get)
        if GROUP_OF[top_label] != winner:
            return "safe"
    return VALENCE[winner]


def average(score_dicts):
    return {label: sum(d[label] for d in score_dicts) / len(score_dicts) for label in score_dicts[0]}


def combine(outcomes, method):
    if "safe" in outcomes:
        return "safe"
    positives, negatives = outcomes.count("positive"), outcomes.count("negative")
    if method == "any-negative":
        return "negative" if negatives else ("positive" if positives else "neutral")
    # majority
    if negatives > positives:
        return "negative"
    if positives > negatives:
        return "positive"
    return "neutral" if positives == 0 else "safe"


def ideal(combo):
    if "negative" in combo:
        return "negative"
    return "positive" if "positive" in combo else "neutral"


def main():
    data = json.loads((HERE / "test_answers.json").read_text(encoding="utf-8"))
    questions = data["questions"]
    model = GoEmotionsModel(MODEL_DIR)

    single = {(q["questionId"], kind): model.scores(q["answers"][kind]) for q in questions for kind in KINDS}
    combos = list(itertools.product(KINDS, repeat=3))
    joined = {combo: model.scores(" ".join(q["answers"][k] for q, k in zip(questions, combo))) for combo in combos}

    rules = ["sum", "mean", "top1", "top2", "top3", "top5"]
    strategies = []
    for rule in rules:
        for umbrella in (False, True):
            tag = f"{rule}{' + umbrella' if umbrella else ''}"
            strategies.append((f"Joined text, {tag}",
                               lambda c, r=rule, u=umbrella: classify(joined[c], r, u)))
            for method in ("majority", "any-negative"):
                strategies.append((f"Per answer, {method}, {tag}",
                                   lambda c, r=rule, u=umbrella, m=method: combine(
                                       [classify(single[(q["questionId"], k)], r, u) for q, k in zip(questions, c)], m)))
            strategies.append((f"Per answer, average scores, {tag}",
                               lambda c, r=rule, u=umbrella: classify(
                                   average([single[(q["questionId"], k)] for q, k in zip(questions, c)]), r, u)))

    rows = []
    for name, strategy in strategies:
        harmful = total = mixed_harmful = mixed_total = safe = 0
        outcomes = {}
        for combo in combos:
            outcome = strategy(combo)
            outcomes[combo] = outcome
            is_mixed = "positive" in combo and "negative" in combo
            is_harmful = outcome == "positive" and "negative" in combo
            is_error = outcome != ideal(combo)
            harmful += is_harmful
            total += is_error
            safe += outcome == "safe"
            mixed_harmful += is_mixed and is_harmful
            mixed_total += is_mixed and is_error
        single_correct = sum(classify(single[(q["questionId"], k)], name_rule(name), "umbrella" in name) == k
                             for q in questions for k in KINDS)
        rows.append((harmful, total, name, mixed_harmful, mixed_total, safe, single_correct, outcomes))

    rows.sort(key=lambda row: (row[0], row[1]))
    lines = ["# Strategy Comparison: reducing harm on mixed answer sets", "",
             "Same model and the same 9 test answers as combination_report.md. 27 orderings per strategy;",
             "12 of them are **mixed** (contain both a strong positive and a strong negative answer).", "",
             "**Proposed error definitions** (change them in compare_strategies.py if you disagree):",
             "- *Ideal outcome*: negative if any answer is strongly negative; otherwise positive if any is positive; otherwise neutral.",
             "- *Harmful*: the outcome is **positive** although at least one answer is strongly negative.",
             "- *Total errors*: outcome differs from the ideal. A safe-bag outcome counts as a miss, never as harmful.",
             "- *Single answers*: how many of the 9 answers, scored alone, get their own label (positive/neutral/negative).", "",
             "Sorted by harmful errors, then total errors.", "",
             "| # | Strategy | Harmful (of 27) | Total errors (of 27) | Mixed: harmful (of 12) | Mixed: errors (of 12) | Safe-bag outcomes | Single answers correct (of 9) |",
             "|---|---|---|---|---|---|---|---|"]
    for rank, (harmful, total, name, mh, mt, safe, sc, _) in enumerate(rows, 1):
        lines.append(f"| {rank} | {name} | {harmful} | {total} | {mh} | {mt} | {safe} | {sc} |")

    baseline = next(row for row in rows if row[2] == "Joined text, sum + umbrella")
    best = rows[0]
    lines += ["", f"## Outcomes per combination: baseline vs. top-ranked", "",
              f"Baseline = **{baseline[2]}** (what combination_report.md used). Top-ranked = **{best[2]}**.", "",
              "| Combo | Ideal | Baseline | Top-ranked |", "|---|---|---|---|"]
    for combo in combos:
        code = "".join(SHORT[k] for k in combo)
        lines.append(f"| {code} | {ideal(combo)} | {baseline[7][combo]} | {best[7][combo]} |")
    lines += ["", "## Caveats", "",
              "- Only 9 hand-written answers (3 per control question). Results show *tendencies*, not real-world accuracy.",
              "- Any-negative rules get 0 harmful errors here partly because every single negative answer was classified correctly;",
              "  with real players, one misread answer passes straight through.",
              "- Per-answer strategies run the model 3 times instead of once."]

    out = HERE / "results" / "strategy_comparison.md"
    out.write_text("\n".join(lines) + "\n", encoding="utf-8")
    print(f"Wrote {out}")
    for row in rows[:8]:
        print(row[0], row[1], row[2], "| mixed", row[3], row[4], "| safe", row[5], "| single", row[6])


def name_rule(name):
    for part in name.replace("+ umbrella", "").split(","):
        part = part.strip()
        if part in ("sum", "mean") or part.startswith("top"):
            return part
    raise ValueError(name)


if __name__ == "__main__":
    main()
