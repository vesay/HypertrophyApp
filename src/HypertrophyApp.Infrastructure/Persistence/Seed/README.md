# Exercise library seed

`exercises.json` is the public-domain dataset from
[yuhonas/free-exercise-db](https://github.com/yuhonas/free-exercise-db) (Unlicense),
snapshot of commit `f00c92c7dcf1216a928a52c3706c7ce8e2f71ed5` (876 exercises).

Only the JSON data is vendored here. The start/end position photos are **not** copied into this
repository; they are loaded from the original repository through the jsDelivr CDN
(see `ExerciseLibrary:ImageBaseUrl` in `appsettings.json`). The dataset does not state a license for the
photos, so for a commercial product swap `ImageBaseUrl` to a licensed media source.
