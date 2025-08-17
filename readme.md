# Introduction

cxp is a wrapper around git that tracks command usage and awards XP and achievements.

# Download

Compiled downloads are not available.

# Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/cxp

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

# Usage

cxp has two modes of working;
a) You can call cxp.exe directly
b) You can rename cxp to git and use the appsettings.json file to configure the path to the real git executable

In either case, cxp is used by passing git commands, e.g., `cxp add myfile.txt`. cxp tracks commands used and awards XP. An achievement dialog is displayed in the console when an achievement is unlocked.

There are a few commands that are not passed through to git:

`cxp profile` - displays your current XP, level, and achievements
`cxp profile --stats` - displays your current XP, level, and achievements, and additional stats on command usage
`cxp profile --clear` - clears all tracking

# Customisation

Various settings related to achievements and XP awards can be customised in the files below, stored in the `cxpconfig` directory.

- achievements.json

Contains the names of achievements and the XP award.

The milestones section tracks total command usage, and additional milestones can be added, e.g., a new commit milestone for 3 commits can be added:
```json
"commit_3": {
      "name": "Apprentice",
      "description": "Complete 3 commits.",
      "xp_reward": 50
}
```

The combos section track commits on consecutive days, and additional combos can be added, e.g, a new milestone for commits on consecutive days can be added:
```json
"combo_10": {
      "name": "Disciplined Monk",
      "description": "Commit on 10 consecutive days.",
      "xp_reward": 300
}
```

The hourly section tracks commits over the course of the day, and an additional hourly achievement can be added, e.g.
```json
"hour_13": {
      "name": "Lunchday Friar",
      "description": "Commit code between 1 PM and 2 PM.",
      "xp_reward": 90
}
```

The builder section tracks the number of files added in an individual commit, and a new builder achievement can be added, e.g.
```json
"builder_5": {
      "name": "Novice Crafter", 
      "description": "Add 5 files to the repository.",
      "xp_reward": 50
}
```

The destroyer section tracks the number of files deleted in an individual commit, and a new destroyer achievement can be added, e.g.
```json
"destroyer_5": {
      "name": "Novice Destroyer",
      "description": "Delete 5 files from the repository.",
      "xp_reward": 50
}
```


- levels.json

Contains the level name and the total XP required to reach that level.


- xp.json

Contains the XP award for each operation, per level.

# Licencing

cxp is licensed under the MIT license. Full license details are available in the license.md

# Thanks

This project is inspired by [git achievements](https://github.com/lennart/git-achievements) and [gg-gamify](https://github.com/DeerYang/git-gamify)
