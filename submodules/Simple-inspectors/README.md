# Simple inspectors

This package is aimed into helping people making custom material inspectors by making the process a bit easier and giving them tools to easily organize their inspectors.

Still in development and missing features.

## AutoInspector (A.K.A. lazy mode)

Using the autoInspector is the easiest way to have an inspector that doesn't look horrible, if you follow some simple setups.

The only thing you need to edit is your shader (YAY) by adding the autoInspector as your inspector using this line at the very end: CustomEditor "Cibbi.SimpleInspectors.AutoInspector"
          
And you're done, well, *almost*. To effectively take advantage of the inspector you need some simple naming conventions so the inspector can understand what are you trying to display. 
Is as simple as renaming the display name of your variables.
You can add the following tags as part of the display name, in whatever position you feel like as long as it is inside the name (yes even inbetween a word, but in that case *you should question what's wrong with yourself).*

- (Texture): use this in every texture you have, *seriously, use it in every texture, is like the main reason to use this whole thing.*
- (Extra): use this on up 2 properties immediately after a declared texture property, makes so it packs the properties into a single line, so you can have a texture, it's color, and a float value, all in a single line. ~~Is also the only way right now to not make color properties look like an abortion.~~

- many more i will add when i *feel like doing it*
