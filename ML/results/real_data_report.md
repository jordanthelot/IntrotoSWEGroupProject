# Real-Data Test: single-answer valence accuracy on GoEmotions (test split)

5427 real Reddit comments with human emotion labels, held out from the model's training.
True valence counts: {'negative': 1184, 'positive': 2026, 'neutral': 2139, 'mixed': 78}. Mixed comments (positive AND negative labels) are excluded from accuracy and listed separately.

**Harmful rate** = share of truly negative comments read as positive (the error that matters most).
**Negative recall** = share of truly negative comments read as negative.

## Group scoring: mean

- Accuracy: **68.1%** (3642/5349)
- Negative recall: **32.3%**
- Harmful rate: **2.3%** (27 of 1184 negative comments read as positive)
- Mixed comments were read as: {'negative': 19, 'neutral': 26, 'positive': 33}

| True \ Predicted | positive | neutral | negative |
|---|---|---|---|
| positive | 1231 | 766 | 29 |
| neutral | 75 | 2029 | 35 |
| negative | 27 | 775 | 382 |

**Examples of harmful misses (mean scoring), first 10:**

- "Eff your video - love Canada 🇨🇦 Stupid geolock" (human labels: anger, annoyance)
- "Lovely places to buy from but very hard to get a good price as a seller because of their high rents (and bargaining e..." (human labels: disappointment)
- "Pretty shit tbh with you" (human labels: disgust)
- "Yeah thank you you ungrateful bast....." (human labels: anger)
- "If it's true i'm sad he's leaving but I wish him nothing but the best." (human labels: sadness)
- "Sorry. I am avoiding arguments today. Enjoy the video." (human labels: remorse)
- "Lol it's a public sidewalk, I wouldn't call it their set " (human labels: disapproval)
- "Being lonely without him is far better than being lonely with him. Good line. I'm stealing that one!" (human labels: annoyance)
- "Ah ok, that makes more sense. I don’t think most [NAME] would see your last name and try to make it French, lol." (human labels: disapproval)
- "Oh I know lol I overdid it w that comparison. The point is singers have shit vocal nights but that doesn’t make them ..." (human labels: annoyance)

## Group scoring: sum

- Accuracy: **76.5%** (4091/5349)
- Negative recall: **68.6%**
- Harmful rate: **6.7%** (79 of 1184 negative comments read as positive)
- Mixed comments were read as: {'negative': 26, 'neutral': 6, 'positive': 46}

| True \ Predicted | positive | neutral | negative |
|---|---|---|---|
| positive | 1686 | 258 | 82 |
| neutral | 291 | 1593 | 255 |
| negative | 79 | 293 | 812 |

## Group scoring: top1

- Accuracy: **76.3%** (4079/5349)
- Negative recall: **63.3%**
- Harmful rate: **6.6%** (78 of 1184 negative comments read as positive)
- Mixed comments were read as: {'negative': 26, 'neutral': 7, 'positive': 45}

| True \ Predicted | positive | neutral | negative |
|---|---|---|---|
| positive | 1641 | 324 | 61 |
| neutral | 252 | 1689 | 198 |
| negative | 78 | 357 | 749 |

## Caveats

- These are Reddit comments, the same kind of text the model was trained on, so real player answers (tweet-like, about social media habits) may score lower.
- Human labels are themselves noisy: GoEmotions raters often disagreed.
- Data: GoEmotions by Google Research (Apache 2.0).
