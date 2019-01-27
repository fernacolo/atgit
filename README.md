# What is atgit?

`atgit` is a tiny command line tool for running the same command on multiple Git repositories. It detects Git repositories that are reside in the current directory tree, and run your desired command on each of them.

Yes, I know that there are other tools for that: [gr](https://github.com/mixu/gr), [myrepos](http://myrepos.branchable.com/), [meta](https://www.npmjs.com/package/meta) and possibly others. But I was in need of something that does not depend on Node.js or Perl, and to be installable from the [Nuget Gallery](https://www.nuget.org/).

# Getting started

Please grab the latest version and see installation instructions [here](https://www.nuget.org/packages/atgit/).

Once installed in your Nuget cache, find the exe and run `atgit --add-to-path` to add the tool to your path, then you are good to go. At any time, run `atgit` without parameters to see help.

# Examples

## Example 1: get status of all repositories

Command: `atgit -- git status -sb`

Output:

    c:\repos-home\incom\a-wing
    ## master...origin/master [ahead 3]

    c:\repos-home\incom\b-wing
    ## master...origin/master

    c:\repos-home\incom\x-wing
    ## master...feature/newtorpedo
    ?? src/torpedo.launch

    c:\repos-home\sienar\tie-bomber
    ## master...origin/master [behind 2]

    c:\repos-home\sienar\tie-fighter
    ## master...origin/master

## Example 2: show contents of version.txt at root of every repository

Command: `atgit -- cmd /c "type version.txt"`

Output:

    c:\repos-home\incom\a-wing
    7.2.19

    c:\repos-home\incom\b-wing
    7.2.45

    c:\repos-home\incom\x-wing
    7.5.1

    c:\repos-home\sienar\tie-bomber
    4.0.1

    c:\repos-home\sienar\tie-fighter
    4.0.1

## Example 3: pull all repositories avoiding merge bubbles (i.e. fast-forward)

Command: `atgit -f -- git pull --ff-only`

Output:

    c:\repos-home\incom\a-wing
    Already up to date.

    c:\repos-home\incom\b-wing
    ## master...origin/master
    remote: Incom Repository
    remote: Found 29 objects to send. (91 ms)
    Unpacking objects: 100% (29/29), done.
    From https://incom.rebelfiles.org/fleet/b-wing
       1e139e0..fd98200  master     -> origin/master
    Updating 1e139e0..fd98200
    Fast-forward
     configuration/version.xml            |  2 +-
     src/bwingmain/avionics/radar.cs      | 74 ++++++++++++------------------------
     src/bwingmain/avionics/util.cs       |  4 +-
     3 files changed, 27 insertions(+), 53 deletions(-)

    c:\repos-home\incom\x-wing
    fatal: Not possible to fast-forward, aborting.
    Exit code: 128.

    c:\repos-home\sienar\tie-bomber
    Already up to date.

    c:\repos-home\sienar\tie-fighter
    Already up to date.

## Example 4: see recent commits in all repositories

Command: `atgit -- git log --since=\"5 days ago\" --oneline`

Output:

    c:\repos-home\incom\a-wing
    4d258dff Replaced laser for a lighter version.
    3ea6db42 Small fixes to HUD messages.
    adaeadd2 Release 7.2.19

    c:\repos-home\incom\b-wing
    bd9d71a5 Release 7.2.45

    c:\repos-home\incom\x-wing
    871937e2 More torpedo improvements.
    d139b64a More torpedo improvements.
    6fd7726d First commit of new torpedo system.

    c:\repos-home\sienar\tie-bomber

    c:\repos-home\sienar\tie-fighter

