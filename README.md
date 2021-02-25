For API documentation:
https://github.com/JoshuaLim007/InGameConsole


# IGC for Unity
created by Joshua Lim

## Installation:

Copy this git URL:
>https://github.com/JoshuaLim007/InGameConsole

and paste it into Unity's UPM "Add package from git URL".
https://docs.unity3d.com/Manual/upm-ui-giturl.html

**OR**

Download this onto your computer. And install it via local drive.
https://docs.unity3d.com/Manual/upm-ui-local.html


## Getting started

Go to the top of the Unity window and navigate to **InGameConsole -> InGameConsoleManager**
Here you will have access to the console settings. Press **Toggle Preview** to edit these settings. Make sure to press **Save** before exiting.

The console will be **automatically created** when you enter play mode; there is no need to create the prefab of the console. Every scene in build settings will have a console after the project is built.


### Important Settings
**Make sure all the assemblies with your console commands are checked in the settings!** This will allow the program to find your console commands in the assemblies.


## IGC documentation

 **Namespace:**
To access the api you will need to according namespace:
>using namespace Lim.InGameConsole;

 **Creating Custom Commands:**
Use **[ConsoleCommand]** method attribute:
```
public class TestScript{
	[ConsoleCommand]
	public static void method_number(){
		//your code
	}
}
```
 **Calling Commands via IGC:**
Commands will be formatted in class to method name hiearchy. 

For:

```
public class className{
	[ConsoleCommand]
	public static void methodName(int amount, string name){
		//your code
	}
}
```
You will type:
>className.methodName(int32, string)

### GameConsole API:


**GameConsole.OnConsoleVisibilityChange**
>public static OnTrigger **OnConsoleVisibilityChange**

Delegate triggered when the console visibility is changed.

**GameConsole.IsActive**
>public static bool **IsActive**

Returns console visibility state.

**GameConsole.SetActive**
>public static void **SetActive**(bool **state**)

Sets console visibility.

**GameConsole.Print**
>public static void **Print**(object **message**)

Prints to console.

**GameConsole.GetFamiliarCommands**
>public static List<string<l>>**GetFamiliarCommands**(string **className**, string **methodName**)

Gets class name and method name of the custom command and returns all similar commands.

**GameConsole.CallCommand**
>public static void **GetFamiliarCommands**(string **className**, string **methodName**, object[] **arg**)

Calls command with class name and method name and its parameters.

**GameConsole.History**
>public static Queue<CommandHistory<l>> **History**

Contains all previous command prints

**GameConsole.Commands**
>public static Dictionary<string, Dictionary<string, MethodInfo>> **Commands**

Contains all commands grouped by command class name then command method name.

## FAQ
#### Can I change the looks of the console?
No as of now, you cannot change the looks except for the field sizes.
#### Will the release/debug build contain the console?
Yes of course. It will be automatically created.
#### Will you on a date with me?
**😳** jk. no one asks me that **😔**

Created by Joshua Lim
