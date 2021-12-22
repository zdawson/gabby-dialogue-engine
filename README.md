# Gabby Dialogue Engine
### A game dialogue engine for Unity using the Gabby Dialogue Language.
### [**Learn more about using Gabby here!**](https://potassium-k.itch.io/gabby)
----------

<br>

Gabby is a language made for game dialogue. It makes it easy to build games and visual novels with dialogue at their core.

<br>

Working with Gabby is easy:

```
[Gabby.HelloWorld]

    (Gabby) Hello, world!
    -       ...
    -       Hello?

    (Kay)   Hey Gabby.

    (Gabby) Hi!
```
<br>

[**Play the sample in your browser on itch.io here!**](https://potassium-k.itch.io/gabby)

Learn how to use [Gabby](https://github.com/zdawson/gabby), check out the [game sample repository](https://github.com/zdawson/gabby-dialogue-sample), and grab the [vscode plugin](https://marketplace.visualstudio.com/items?itemName=PotassiumK.gabby-lang)!

<br>

## Getting Started

It's easy to integrate Gabby into your Unity project.

The recommended approach is to add the project through the Unity package manager.

[See "Installing from a Git URL" for details.](https://docs.unity3d.com/Manual/upm-ui-giturl.html)

Alternatively, you can clone the repository as a submodule under `Packages/`.

```
git submodule add https://github.com/zdawson/gabby-dialogue-engine.git
```

Both of these options make it easy to keep the package up to date.

<br>

Once you've added the package to your project, you can start using Gabby - Gabby Dialogue Files (.gab) will automatically be imported.

To start playing a dialogue, you'll need a [`DialogueEngine`](Runtime/DialogueEngine.cs) instance - most likely, you'll want to extend [`SimpleDialogueSystem`](Runtime/SimpleDialogueSystem.cs) and build your dialogue system from there. You can then load your dialogue scripts with `AddScript()` and play a dialogue with `PlayDialogue()`.

<br>

[Check out the sample dialogue system for reference.](https://github.com/zdawson/gabby-dialogue-sample/blob/master/Assets/GameSample/Scripts/GameSampleDialogueSystem.cs)

<br>

The gist of what you will need is:
- Your own dialogue UI, with its own event handling, text animation, and so on. (or use the sample's UI as a starting point!)
- Your own dialogue system to handle dialogue events raised by the dialogue engine and feed them to your UI.
- A dialogue script and dialogue to play.

As you write more advanced dialogues, you'll also want to write some scripting handlers to add support for actions and conditionals, and an options UI to present choices to the player.

<br>

## Documentation

Documentation for the engine plugin will be added here soon.

To learn how to use language, check out the [Gabby repository](https://github.com/zdawson/gabby).

<br>

## License

This project is released under the MIT license. [You can read about this license here.](https://choosealicense.com/licenses/mit/)

<br>

## Links

- [Gabby](https://github.com/zdawson/gabby) - Learn how to write Gabby and check out the language spec.

- [Gabby Dialogue Engine](https://github.com/zdawson/gabby-dialogue-engine) - This repository. A Gabby Dialogue Engine implementation for Unity. Clone this directly if you don't want the sample as well.

- [Gabby Dialogue Sample](https://github.com/zdawson/gabby-dialogue-sample) - An interactive sample for the Gabby Dialogue Language made with Unity.

- [VSCode Plugin](https://marketplace.visualstudio.com/items?itemName=PotassiumK.gabby-lang) - Syntax highlighting for Visual Studio Code.

- [itch.io](https://potassium-k.itch.io/gabby) - The game sample, hosted on itch.io.
