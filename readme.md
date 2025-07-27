# Introduction

cxp is a wrapped around git that tracks command usage and awards XP and achivements.

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

In either case cxp is used by passing git commands e.g. `cxp add myfile.txt`. cxp tracks commands used and awards xp. An achivement dialog is displayed in the console when an achievement is unlocked.

There are a few commands that are not passed through to git:

`cxp profile` - displays your current xp, level and achievements
`cxp profile --stats` - displays your current xp, level and achievements and additional stats on command usage
`cxp profile --clear` - clears all tracking

# Customisation

Various settings related to achievements and xp awards can be customised in the files below, stored in the `cxpconfig` directory.

- achievements.json

Contains the name of achievements and the xp award.

The milestones section track total command usage and additional milestones can be added, e.g. a new commit milestone for 3 commits can be added:
```json
"commit_3": {
      "name": "Apprentice",
      "description": "Complete 3 commits.",
      "xp_reward": 50
}
```

The combos section track commits on consecutive days and additional combos can be added e.g. a new milestone for commits on consecutive days can be added:
```json
"combo_10": {
      "name": "Disciplined Monk",
      "description": "Commit on 10 consecutive days.",
      "xp_reward": 300
}
```

The hourly section tracks commits over the course of the day and a additional hourly achievement can be added e.g.
```json
"hour_13": {
      "name": "Lunchday Friar",
      "description": "Commit code between 1 PM and 2 PM.",
      "xp_reward": 90
}
```

The builder section tracks the number of files added in an individual commit and a new builder achievement can be added e.g.
```json
"builder_5": {
      "name": "Novice Crafter", 
      "description": "Add 5 files to the repository.",
      "xp_reward": 50
}
```

The detstroyer section tracks the number of files deleted in an individual commit and a new destroyer achivement can be added e.g.
```json
"destroyer_5": {
      "name": "Novice Destroyer",
      "description": "Delete 5 files from the repository.",
      "xp_reward": 50
}
```


- levels.json

Contains the level name and the total xp required to reach that level.


- xp.json

Contains the xp award for each operation, per level.

# Licencing

cxp is licenced under the MIT license. Full licence details are available in license.md

# Thanks

This project is inspired by [git achievements](https://github.com/lennart/git-achievements) and [gg-gamify](https://github.com/DeerYang/git-gamify)