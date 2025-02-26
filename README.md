## Quantum Codegen External
A standalone console app wrapper around Photon Quantum Unity codegen. Allows triggering codegen outside of the Unity editor, which allows significant workflow improvements for IDEs and VCS.

### IDEs
Currently after editing `.qtn` files in an IDE, the user must switch back to the Unity editor and either trigger an asset refresh or run codegen (which force-triggers an asset refresh out-of-the-box). This is a pretty annoying workflow.

With a standalone tool it's very easy to configure an IDE or some file watcher to run codegen after changes (or additions/removals) to `.qtn` files are detected. In this way you can edit a `.qtn` file and immediately see the results reflected in your IDE without the need to switch back to the editor (or even have it open at all).

### VCS
It's fairly common to have compile errors after merging two branches that both contain changes to `.qtn` files as the generated `.cs` files are usually stale. The fix is conceptually very simple: just run codegen.

However running codegen requires opening the editor, which is time consuming in any non-trivial project. The editor also opens in safe mode due to the compile errors, which is intimidating to most users. The user must exit safe mode and run codegen (if it isn't triggered automatically after exiting safe mode).

If we can instead run codegen automatically post-merge (probably via git hooks and/or some repo-side automation) then we can avoid the issue altogether (barring genuine compile errors not stemming from stale generated files, ofc).

