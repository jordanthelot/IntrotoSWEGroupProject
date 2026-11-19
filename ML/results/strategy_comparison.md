# Strategy Comparison: reducing harm on mixed answer sets

Same model and the same 9 test answers as combination_report.md. 27 orderings per strategy;
12 of them are **mixed** (contain both a strong positive and a strong negative answer).

**Proposed error definitions** (change them in compare_strategies.py if you disagree):
- *Ideal outcome*: negative if any answer is strongly negative; otherwise positive if any is positive; otherwise neutral.
- *Harmful*: the outcome is **positive** although at least one answer is strongly negative.
- *Total errors*: outcome differs from the ideal. A safe-bag outcome counts as a miss, never as harmful.
- *Single answers*: how many of the 9 answers, scored alone, get their own label (positive/neutral/negative).

Sorted by harmful errors, then total errors.

| # | Strategy | Harmful (of 27) | Total errors (of 27) | Mixed: harmful (of 12) | Mixed: errors (of 12) | Safe-bag outcomes | Single answers correct (of 9) |
|---|---|---|---|---|---|---|---|
| 1 | Per answer, any-negative, mean | 0 | 0 | 0 | 0 | 0 | 9 |
| 2 | Per answer, any-negative, sum | 0 | 1 | 0 | 0 | 0 | 8 |
| 3 | Per answer, any-negative, sum + umbrella | 0 | 1 | 0 | 0 | 0 | 8 |
| 4 | Per answer, any-negative, top1 | 0 | 1 | 0 | 0 | 0 | 8 |
| 5 | Per answer, any-negative, top1 + umbrella | 0 | 1 | 0 | 0 | 0 | 8 |
| 6 | Per answer, any-negative, top2 | 0 | 1 | 0 | 0 | 0 | 8 |
| 7 | Per answer, any-negative, top2 + umbrella | 0 | 1 | 0 | 0 | 0 | 8 |
| 8 | Per answer, any-negative, top3 | 0 | 1 | 0 | 0 | 0 | 8 |
| 9 | Per answer, any-negative, top3 + umbrella | 0 | 1 | 0 | 0 | 0 | 8 |
| 10 | Per answer, any-negative, top5 | 0 | 1 | 0 | 0 | 0 | 8 |
| 11 | Per answer, any-negative, top5 + umbrella | 0 | 1 | 0 | 0 | 0 | 8 |
| 12 | Per answer, any-negative, mean + umbrella | 0 | 9 | 0 | 2 | 9 | 8 |
| 13 | Per answer, majority, mean | 3 | 9 | 3 | 9 | 6 | 9 |
| 14 | Per answer, majority, mean + umbrella | 3 | 16 | 3 | 9 | 13 | 8 |
| 15 | Per answer, average scores, mean | 4 | 19 | 4 | 9 | 0 | 9 |
| 16 | Per answer, average scores, mean + umbrella | 4 | 20 | 4 | 10 | 5 | 8 |
| 17 | Joined text, mean + umbrella | 5 | 11 | 5 | 10 | 6 | 8 |
| 18 | Per answer, majority, sum | 5 | 12 | 5 | 9 | 6 | 8 |
| 19 | Per answer, majority, sum + umbrella | 5 | 12 | 5 | 9 | 6 | 8 |
| 20 | Per answer, majority, top1 | 5 | 12 | 5 | 9 | 6 | 8 |
| 21 | Per answer, majority, top1 + umbrella | 5 | 12 | 5 | 9 | 6 | 8 |
| 22 | Per answer, majority, top2 | 5 | 12 | 5 | 9 | 6 | 8 |
| 23 | Per answer, majority, top2 + umbrella | 5 | 12 | 5 | 9 | 6 | 8 |
| 24 | Per answer, majority, top3 | 5 | 12 | 5 | 9 | 6 | 8 |
| 25 | Per answer, majority, top3 + umbrella | 5 | 12 | 5 | 9 | 6 | 8 |
| 26 | Per answer, majority, top5 | 5 | 12 | 5 | 9 | 6 | 8 |
| 27 | Per answer, majority, top5 + umbrella | 5 | 12 | 5 | 9 | 6 | 8 |
| 28 | Joined text, sum + umbrella | 6 | 11 | 6 | 10 | 4 | 8 |
| 29 | Joined text, top3 + umbrella | 6 | 11 | 6 | 10 | 4 | 8 |
| 30 | Joined text, top5 + umbrella | 6 | 11 | 6 | 10 | 4 | 8 |
| 31 | Joined text, mean | 7 | 7 | 7 | 7 | 0 | 9 |
| 32 | Per answer, average scores, sum + umbrella | 7 | 19 | 7 | 10 | 11 | 8 |
| 33 | Per answer, average scores, top3 + umbrella | 7 | 20 | 7 | 11 | 9 | 8 |
| 34 | Per answer, average scores, top5 + umbrella | 7 | 20 | 7 | 11 | 11 | 8 |
| 35 | Per answer, average scores, top2 + umbrella | 7 | 21 | 7 | 12 | 9 | 8 |
| 36 | Joined text, sum | 8 | 9 | 8 | 8 | 0 | 8 |
| 37 | Per answer, average scores, sum | 8 | 9 | 8 | 8 | 0 | 8 |
| 38 | Joined text, top1 | 8 | 9 | 8 | 8 | 0 | 8 |
| 39 | Joined text, top1 + umbrella | 8 | 9 | 8 | 8 | 0 | 8 |
| 40 | Joined text, top3 | 8 | 9 | 8 | 8 | 0 | 8 |
| 41 | Joined text, top5 | 8 | 9 | 8 | 8 | 0 | 8 |
| 42 | Joined text, top2 + umbrella | 8 | 11 | 8 | 10 | 2 | 8 |
| 43 | Per answer, average scores, top1 | 8 | 19 | 8 | 10 | 0 | 8 |
| 44 | Per answer, average scores, top1 + umbrella | 8 | 19 | 8 | 10 | 0 | 8 |
| 45 | Per answer, average scores, top5 | 9 | 11 | 9 | 9 | 0 | 8 |
| 46 | Per answer, average scores, top3 | 9 | 13 | 9 | 10 | 0 | 8 |
| 47 | Joined text, top2 | 10 | 11 | 10 | 10 | 0 | 8 |
| 48 | Per answer, average scores, top2 | 10 | 15 | 10 | 11 | 0 | 8 |

## Outcomes per combination: baseline vs. top-ranked

Baseline = **Joined text, sum + umbrella** (what combination_report.md used). Top-ranked = **Per answer, any-negative, mean**.

| Combo | Ideal | Baseline | Top-ranked |
|---|---|---|---|
| PPP | positive | positive | positive |
| PP0 | positive | positive | positive |
| PPN | negative | safe | negative |
| P0P | positive | positive | positive |
| P00 | positive | positive | positive |
| P0N | negative | safe | negative |
| PNP | negative | positive | negative |
| PN0 | negative | safe | negative |
| PNN | negative | negative | negative |
| 0PP | positive | positive | positive |
| 0P0 | positive | positive | positive |
| 0PN | negative | positive | negative |
| 00P | positive | positive | positive |
| 000 | neutral | positive | neutral |
| 00N | negative | negative | negative |
| 0NP | negative | safe | negative |
| 0N0 | negative | negative | negative |
| 0NN | negative | negative | negative |
| NPP | negative | positive | negative |
| NP0 | negative | positive | negative |
| NPN | negative | positive | negative |
| N0P | negative | positive | negative |
| N00 | negative | negative | negative |
| N0N | negative | negative | negative |
| NNP | negative | negative | negative |
| NN0 | negative | negative | negative |
| NNN | negative | negative | negative |

## Caveats

- Only 9 hand-written answers (3 per control question). Results show *tendencies*, not real-world accuracy.
- Any-negative rules get 0 harmful errors here partly because every single negative answer was classified correctly;
  with real players, one misread answer passes straight through.
- Per-answer strategies run the model 3 times instead of once.
