# Combination Report: GoEmotions (SamLowe/roberta-base-go_emotions-onnx, full precision)

Answers to the 3 control questions are joined with a space and classified as one text.
Scores are per-label sigmoid probabilities (multi-label). Group = sum of member-label scores,
using Google's GoEmotions sentiment mapping. **DISAGREE** = the top emotion is not in the winning group.
Combination codes: P = strong positive, 0 = neutral, N = strong negative, in question order (q022, q013, q031).

## Each answer on its own

| Question | Kind | Answer | Top 3 | Winning group | Check |
|---|---|---|---|---|---|
| q022 | positive | honestly pretty good, feel caught up w my friends and kinda recharged lol | amusement 0.78, admiration 0.67, joy 0.19 | positive | ok |
| q022 | neutral | idk normal i guess, just go do whatever's next | approval 0.50, confusion 0.32, optimism 0.22 | positive | ok |
| q022 | negative | lowkey awful. drained and anxious, like i wasted my whole night again | nervousness 0.24, fear 0.22, sadness 0.11 | negative | ok |
| q013 | positive | in bed but tbh i love it, best part of my day, so many funny vids | love 0.83, amusement 0.39, joy 0.31 | positive | ok |
| q013 | neutral | usually in bed or on the bus | neutral 0.96, approval 0.03, realization 0.01 | neutral | ok |
| q013 | negative | in bed every night and i hate it, i cant stop and i feel so gross after | disgust 0.55, anger 0.27, annoyance 0.21 | negative | ok |
| q031 | positive | my best friend who moved away!! we send memes all day and it makes me so happy | joy 0.76, sadness 0.30, disappointment 0.04 | positive | ok |
| q031 | neutral | some cousins i guess, we follow each other | neutral 0.71, confusion 0.15, realization 0.08 | neutral | ok |
| q031 | negative | nobody really. everyone posts their perfect lives and i just feel lonely and left out | sadness 0.79, disappointment 0.22, neutral 0.04 | negative | ok |

## All 27 orderings

| Combo | Top 3 | Group sums (pos/neg/amb/neu) | Winning group (sum) | Winning group (max) | Check |
|---|---|---|---|---|---|
| PPP | love 0.61, amusement 0.50, joy 0.42 | pos 1.91 / neg 0.11 / amb 0.02 / neu 0.00 | positive | positive | ok |
| PP0 | love 0.65, amusement 0.56, admiration 0.53 | pos 2.04 / neg 0.03 / amb 0.02 / neu 0.01 | positive | positive | ok |
| PPN | sadness 0.42, love 0.42, amusement 0.28 | pos 1.04 / neg 0.62 / amb 0.03 / neu 0.00 | positive | negative | **DISAGREE** |
| P0P | amusement 0.63, joy 0.46, admiration 0.45 | pos 1.65 / neg 0.13 / amb 0.03 / neu 0.01 | positive | positive | ok |
| P00 | amusement 0.82, admiration 0.52, joy 0.16 | pos 1.61 / neg 0.02 / amb 0.02 / neu 0.01 | positive | positive | ok |
| P0N | sadness 0.34, amusement 0.30, admiration 0.22 | pos 0.64 / neg 0.56 / amb 0.03 / neu 0.01 | positive | negative | **DISAGREE** |
| PNP | admiration 0.32, amusement 0.27, joy 0.19 | pos 0.86 / neg 0.45 / amb 0.02 / neu 0.00 | positive | positive | ok |
| PN0 | admiration 0.26, amusement 0.25, annoyance 0.23 | pos 0.58 / neg 0.72 / amb 0.02 / neu 0.01 | negative | positive | **DISAGREE** |
| PNN | sadness 0.30, disappointment 0.24, admiration 0.15 | pos 0.30 / neg 0.83 / amb 0.02 / neu 0.01 | negative | negative | ok |
| 0PP | love 0.77, joy 0.70, amusement 0.28 | pos 1.93 / neg 0.10 / amb 0.04 / neu 0.01 | positive | positive | ok |
| 0P0 | love 0.82, amusement 0.44, joy 0.43 | pos 1.99 / neg 0.04 / amb 0.08 / neu 0.01 | positive | positive | ok |
| 0PN | love 0.64, sadness 0.52, disappointment 0.17 | pos 0.94 / neg 0.81 / amb 0.05 / neu 0.01 | positive | positive | ok |
| 00P | joy 0.82, approval 0.07, sadness 0.06 | pos 1.06 / neg 0.15 / amb 0.08 / neu 0.03 | positive | positive | ok |
| 000 | approval 0.37, confusion 0.36, neutral 0.25 | pos 0.49 / neg 0.02 / amb 0.44 / neu 0.25 | positive | positive | ok |
| 00N | sadness 0.51, disappointment 0.31, neutral 0.06 | pos 0.08 / neg 0.91 / amb 0.07 / neu 0.06 | negative | negative | ok |
| 0NP | joy 0.40, sadness 0.18, anger 0.15 | pos 0.52 / neg 0.64 / amb 0.04 / neu 0.01 | negative | positive | **DISAGREE** |
| 0N0 | anger 0.44, annoyance 0.36, disgust 0.33 | pos 0.04 / neg 1.27 / amb 0.02 / neu 0.02 | negative | negative | ok |
| 0NN | sadness 0.31, anger 0.25, annoyance 0.23 | pos 0.04 / neg 1.10 / amb 0.03 / neu 0.01 | negative | negative | ok |
| NPP | love 0.68, joy 0.37, sadness 0.18 | pos 1.21 / neg 0.41 / amb 0.04 / neu 0.00 | positive | positive | ok |
| NP0 | love 0.86, joy 0.10, sadness 0.08 | pos 1.10 / neg 0.33 / amb 0.04 / neu 0.00 | positive | positive | ok |
| NPN | love 0.70, sadness 0.48, disappointment 0.16 | pos 0.87 / neg 0.80 / amb 0.04 / neu 0.00 | positive | positive | ok |
| N0P | joy 0.59, sadness 0.15, nervousness 0.13 | pos 0.76 / neg 0.45 / amb 0.04 / neu 0.01 | positive | positive | ok |
| N00 | fear 0.39, nervousness 0.33, annoyance 0.06 | pos 0.08 / neg 0.96 / amb 0.05 / neu 0.04 | negative | negative | ok |
| N0N | sadness 0.45, disappointment 0.24, nervousness 0.13 | pos 0.07 / neg 0.95 / amb 0.03 / neu 0.02 | negative | negative | ok |
| NNP | sadness 0.30, joy 0.30, annoyance 0.10 | pos 0.40 / neg 0.73 / amb 0.03 / neu 0.01 | negative | negative | ok |
| NN0 | disgust 0.42, annoyance 0.29, anger 0.22 | pos 0.04 / neg 1.14 / amb 0.02 / neu 0.01 | negative | negative | ok |
| NNN | sadness 0.36, annoyance 0.19, disappointment 0.17 | pos 0.05 / neg 1.01 / amb 0.02 / neu 0.01 | negative | negative | ok |

## Does order matter? (10 distinct mixes)

| Mix | Orderings | Top emotions seen | Winning groups seen | Top-score range |
|---|---|---|---|---|
| 000 | 1 | approval | positive | 0.37–0.37 |
| 00N | 3 | anger, fear, sadness | negative | 0.39–0.51 |
| 00P | 3 | amusement, joy, love | positive | 0.82–0.82 |
| 0NN | 3 | disgust, sadness | negative | 0.31–0.45 |
| 0NP | 6 | admiration, joy, love, sadness | negative, positive | 0.26–0.86 |
| 0PP | 3 | amusement, love | positive | 0.63–0.77 |
| NNN | 1 | sadness | negative | 0.36–0.36 |
| NNP | 3 | love, sadness | negative, positive | 0.30–0.70 |
| NPP | 3 | admiration, love, sadness | positive | 0.32–0.68 |
| PPP | 1 | love | positive | 0.61–0.61 |
