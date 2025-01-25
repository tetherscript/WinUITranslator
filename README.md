# WinUITranslator
## Translate your WinappSDK WinUI Apps.

> [!IMPORTANT]
> - Requires Visual Studio 2022 with .Net 8 and WinAppSDK 1.6

- This Visual Studio 2022 solution contains a Translator app that translates your WinAppSDK WinUI project to the languages of your choice.  It will scan your project, create the necesary translations, and update your Resource.resw files.
- An OpenAI API translation function is included.  You can translate an app for under $5 USD.
- You can also create your own translation function to call whatever translation service you want, including local and free models!
- Run the included sample apps.  They have already been translated from en-US to de-DE, fr-FR and ar-SA.  You can modify these and re-translate them to see how it works.
- Explore the included static class TLocalized and updated Resource.resw files.
- You only need to include the TLocalized static class in your project.  It allows dynamic text translations, like for a 'Loading {0}, please wait...' status message.  It also allows the retrieval of non-translated resources, such as icon files, colors etc.
- No third party packages or libraries are used.
- Trimming is supported.

> [!NOTE]
> You use the Translator app to translate your target app.

# Why Does this Exist?
I needed to translate several WinAppSDK Winui apps.  I tried some Nuget package tools, but realized that I really don't understand how the resources/translation/localization thing works.  I figured if I made my own tool that can meet the requirements listed below, I would understand.  I posted this as a public repository to share it, and see what ideas/insights other devs may have.

Now that AI is everywhere, I set out to use AI to do the translations, and it works really well.  Really nice.

> [!TIP]
> Take a close look at the packaged and unpackaged sample apps.  It helps clarify the obscure and somewhat undocumented process of setting the language and testing with RTL languages.

<br/><br/><br/>

Note: the following is also displayed in the tabs in the Translator app.

# Introduction

## The Requirements

I am building an app and will be translating it to other languages, and:

1. I want to translate certain `.xaml` elements and dynamically generated text to varying physical lengths.
2. I will not change languages while the app is running and expect to see the changes before restarting the app.
3. I don't want to introduce additional code dependencies or trimming constraints.
4. Translation changes must be diff-able in Git.
5. It needs to be able to translate Packaged or Unpackaged apps.
6. No additional runtimes or packages will be required for publishing the app.
7. I need to specify certain icons and colors for certain languages.
8. I need to literally be able to add several new languages TODAY, press a few buttons, and it's translated and releasable TODAY.

This Translator app can do that. Even #8.

## Solution

1. In your app to be translated (the "target"), add a `\Translator` folder and some files. Adjust your `.xaml` elements and dynamic text as shown in the `x:Uid`'s and `TLocalized.Get()` tabs.
2. In this Translator app, set the target to that app, scan, then translate.
3. Build and run the target app. The translations are now visible.

## Try it with the Sample Apps

1. Run the packaged or unpackaged sample app in VS2022.
2. Run this Translator app in another instance of VS2022, or just run the deployed version from the start menu.
3. The sample app is ready to convert. Target, scan, and translate, then run the sample app to see the changes.


## Ideal Real-World Usage

1. Continue developing and debugging your app.
2. When you are ready to translate, run this Translator app, then run your app again to see the changes.
3. Tweak your app code, translate, tweak code, translate. Rinse. Repeat until it is fully translated.

> [!TIP]
> If you are creating an app that will need to be translated, **you need to set it up and test the translation on day one of development.**
> Put a Hello World button, a dynamic text message, and define several languages.
> Translate it. Test the translations under different languages. Understand how it works.>><br/>><br/>
> Then you'll know what you need to do to make your app localized. It will affect your decisions on layouts, text length, and spacing that are difficult to fix later.
> Starting this process of adding `x:Uid`'s, `.Get()`'s, and running the Translator app **after your app is already big is a pain.**
> I know this. Best to start translating on day 1.

<br/><br/><br/>

# The Target

> [!NOTE]
> You have an app project that you want to translate.  That's the Target.

1. Make sure you have added a `\Translator` folder to your target project. This folder will hold translation info unique to that target project. **Do not include this folder and its files in target project builds or publishing** (`build action = none, do not copy`).

2. Try targeting one of the sample apps—they already have the folder and files. Once you scan, then translate, then restart the sample app, you'll see the translations there.

## \Translator Contents

- **Cache.json**  
  Caches translations to save translation costs/latency. Translating will add items to the cache. It won't update existing items. The translated `.resw` files (non `us-EN`) are rebuilt from this cache. You can edit this file if you need to adjust a translation manually or clear the cache. Your edits will persist.

- **Hints.json**  
  Defines the prompts sent to the translator (OpenAI API in this case). Edit this in the Hints tab.

- **Resources.resw**  
  A blank (no data entries) resource file. Don't edit this blank file. You'll be copying this blank file initially to the `/Strings/???/` folders in your target project (`???` = `en-US` or `de-DE`). When you add a new language, create a new folder and add this blank file.

- **Specials.json**  
  Contains non-translatable items like icons, colors, and things that may need to change for some cultures. These are not translated and are added to the necessary `Resources.Resw` files when you translate. You can edit this file.

- **DetectedXamlElements.json**  
  When you do a scan, info about all detected XAML element types defined in `XamlElements.json` will be added here. This is informational only and is not used by Translator once the scan is complete. It is useful for viewing/troubleshooting when an element is not translated. If it isn't in this file, then the scanner did not pick it up.

- **TLocalizedGets.json**  
  When you use the included static class `TLocalized.Get()` with the debugger attached, an entry is created in this file for each unique `.Get()`. When you do a scan, the detected `.xaml` element info will be merged with these `.Get()`'s to produce the `\Strings\en-US\Resources.resw` file. You can edit this file but should not need to.

- **XamlElements.json**  
  These are the XAML elements and properties that can be detected when scanned. If it isn't on this list, scanning won't find it. You must edit this file.

## Summary

You need to create a `/Translator` folder in the target project and add these files to it.

<br/><br/><br/>

# Scanning

- Searches `.xaml` files used by the target project and detects any elements that have a `x:Uid` and are defined in `XamlElements.json`. 
- It combines this with the items in `TLocalizedGets.json` and `Specials.json` and updates the `target/Strings/en-US/Resources.resw`. 
- You put a blank `Resources.resw` there earlier, and its data section has been replaced with the scan results. This `.resw` is the "truth" for what needs to be translated later.
- In that `Resources.resw` file, you'll see that the hint token (`@`, `@@`, `!`, `!!`) have been placed in the comments field. **Don't edit this file.** 
- No other `.resw` files have been touched. That will happen when you translate.
- The contents of `DetectedXamlElements.json` have also been replaced.
> [!NOTE]
> Scanning is the greatest timesaver over other translation methods. You don't need to manually add items the the .resw files.  That is huge.

## Summary

When a scan is complete, you have found the things that need to be translated, but you have not actually translated anything yet.

Here's what scanning the sample app Sample-Packaged looks like.

<img alt="image" src="https://github.com/user-attachments/assets/2c72466e-6bd4-4224-a10d-6db3bc2b7c42" />

<br/><br/><br/>

# Translating

- Reads `target\Strings\en-US\Resources.resw` and calls a **Translation Function** for each item.
- Saves the translation in the cache and other `target\Strings\???\Resources.resw` files, where `??? = de-DE, ar-SA`, etc.
- Adds any required Specials as well.

## Caching Translations

- Translations are cached in `Cache.json` for re-use. 
- If one of those translations is bad and the Translation Function won't return a good result, you can manually edit the `Cache.json` file with your own translation.  
  - This edit will persist and be used when you click Translate next time.

## Translation Hell

- In this Translator app, being 2025 with AI everywhere, translations are **defaulted to the OpenAI API.**
  - For a small commercial app, this costs around **$5 USD in API credits** (as of Jan 2025) to translate.
  - By the time you test translations, re-translate, and fix issues, it might cost around **$20 USD.**
  - The `Cache.json` prevents re-translating the same items repeatedly, reducing costs.

- There is room for optimization, likely by using an Assistant or batch approach to reduce cost and latency.  
  For now, the app sends a **full prompt for each translation item.**

> [!IMPORTANT]
> You can also run translations locally and for free, depending on the model/API/hardware you use.  **AI API specs/prompt designs are wildly non-standard and changing frequently** as of this time, so the app's code is flexible, allowing you to add your own Translator Function using the API of your choice.

## Translator Functions

- Check the source and locate the **Translator Function** section.
- Use the OpenAI Translator Function as an example.
- Add an item to the function picklist and tweak the code to select which function to use based on the selected picklist item.

## Summary

- Your translations are complete, and `.resw` files are updated.
- Start the target app, and you should see the translations.

Here's what translating the sample app Sample-Packaged looks like.

<img alt="image" src="https://github.com/user-attachments/assets/fecfbfa6-1b4b-4580-a77b-e6f4f9eead3c" />

</br></br></br>

# Hints

Let's say we are translating for a photography-related app.  When we send a string like `'Aperture'` to a Translation Function, do we mean aperture as in photography, or aperture as in an opening of some kind?  We need to give the function a **hint** to get the best translation results. 

With human translators, we might have included context in the `.resw` comments. But in this Translator app, we'll use the following **hint tokens**:

## Accuracy vs Cost

- **`@`** - A normal, generic translation.  
  Example: `'@Aperture'` may be translated as an opening of some kind.

- **`!`** - A translation with extra context.  
  Example: `'!Aperture'` is more likely to be translated in a photography context.

### Why not always use `!`?
- Longer prompts cost more.  
- If a generic `@` prompt works, use that instead to save costs.

## Length Constraints

Often, we have limited screen space to display a string, such as the header above a combobox control. It looks good in `en-US`, but translating it may make the string too long, affecting the layout and making it look bad.

To handle this, use the following hint tokens:

- **`@@`** - A normal, generic translation, but where we want the translation to be of similar or shorter length.
- **`!!`** - A translation with extra context, but where we want the translation to be of similar or shorter length.

## How to Create Prompts with These Hints?

- Enter the prompt corresponding to the hint token in the **Hints tab**.
- These hints will then be accessible in the Translation Function.

</br></br></br>

# x:Uids's

## Why Use x:Uid's?

`x:Uid` properties are typically used to identify `.xaml` elements that require translation. In this Translator app, only elements with an `x:Uid` property and specified in `XamlElements.json` will be detected for translation. Additionally, suffixes like `_btn` are required.

## Examples

### 1. Same String: 'Close' but Different Element Types

The suffixes like `_btn` and `_tb` are important because different element types use different properties for their text.  
For example:

- A `Button` uses the `Content` property.
- A `TextBlock` uses the `Text` property.

If you were to use `x:Uid="Close"` for both elements, the target app would fail to start. The suffix (e.g., `_btn`, `_bn`, `_bt`, `_button`) does not matter as long as it is consistent.

**Important Note:**  
- Do not use decimals (`.`) in the `x:Uid`.  
  Example: `x:Uid="Close.btn"` will translate and cache successfully, but the target app will fail to show the translation.  
  The `.` is used to denote the property of the element, and having more than one `.` will cause problems.

#### Example Code:

```
<Button>
  x:Uid="Close_btn"
  Content="@Close"
</Button>

<TextBlock>
  x:Uid="Close_tb"
  Text="@Close"
</TextBlock>
```

2) Same string but different length context and constraints.
You can have the same strings ex. '@Enter your name here', '@@Enter your name here', '!Enter your name here' and '!!Enter your name here' if you need that kind of flexibility.

```
<TextBlock>
  x:Uid="EnterYourNameHere_btn"
  Content="@Enter your name here" //no size constraint
</TextBlock>

<TextBlock>
  x:Uid="EnterYourNameHere_tb"
  Text="@@Enter your name here" //size constrained
</TextBlock>
```

</br></br></br>

# TLocalized.Get()

## Translating dynamic text (non x:Uid)

### How do we translate this status message:  
`Loading {0}, please wait...`

### Solution

TLocalized.Get(string name, string hint, string value);

Dynamic text can't be found in a scan since the text to be translated may be generated at runtime. If the debugger is attached to the target app, the `TLocalized` class loads `\Translator\LocalizedGets.json` at target app startup, adds any requested translations via `.Get()`'s, and saves the text when the app shuts down. This file is later used in the Translation process. Once the translation is complete, this same `.Get()` call will return the translated text.

### Example

```string translatedText = TLocalized.Get("LoadingFile", "@", "Loading {0}, please wait...");```
// In en-DE (German), this call would return "Lade {0}, bitte warten..."

``string displayText = String.Format(translatedText, "myFile.text");``  
// here we fill in the placeholder with a filename

The `{0}` is a placeholder for a parameter in a `String.Format()` call. OpenAI returns the placeholder in the correct location, though you may need to adjust the hint to let it know what to do with placeholders. You can use the same hint tokens as with `.xaml` elements.

The `.Get()` name parameter has a strict format definition as described in `TLocalizedDef.IsValidXamlIdentifier()`. So you can't use periods in the name, for example.

### Using the class in the target project

You can find `\Common\TLocalized.cs` in this Translator app and copy it or link to it in your target app. It has a `TeeLocalized` namespace. 

> [!NOTE]
> The names are weird to hopefully avoid naming collisions when you include this class in your target project.
> It's named `TLocalized` instead of `Localized` because that's how it should be—at least in the old Delphi days, which ironically was written by the same guy who made C#. Small world.

### More examples

In the sample apps, there are calls to `TLocalized.Get()` that dump the results to the log.

### Is TLocalized Trimming-compatible?

Yes, it is compatible with trimming since it does not use reflection for its JSON calls.

<br/><br/><br/>

# Specials

## Handling icons, colors and other non-translatables

> [!CAUTION]
> Your app could run in languages where, for example, an icon, color, or other item needs to be different from other languages.
> A classic example is using white as a “safe” or “neutral” color in an interface. In many Western cultures, white is associated with cleanliness, simplicity, and purity. However, in several East Asian cultures (such as in China), white is traditionally linked to mourning and death, making it an unappealing choice in certain contexts.
> </br></br>
> Similarly, a well-known icon example is the thumbs-up gesture. While it’s widely seen as a positive “like” or “approval” symbol in much of the world, it can carry offensive or vulgar connotations in some Middle Eastern and West African cultures.
> In both cases, it’s worth doing a bit of research or user testing for each target market to avoid introducing symbols or colors that may come across as inappropriate or off-putting.

### Solution

Add these items to `\Translator\Specials.json`, with one entry for each item and language. These items will be copied into the correct `Resource.resw`s when you do a Translate.

### Example

You have four languages, but `ar-SA` should have a different icon.

`Specials.json`

```json
[
  {
    "Key": "SettingsIcon",
    "Value": "settings1.ico",
    "culture": "en-US"
  },
  {
    "Key": "SettingsIcon",
    "Value": "settings1.ico",
    "culture": "de-DE"
  },
  {
    "Key": "SettingsIcon",
    "Value": "settings1.ico",
    "culture": "fr-FR"
  },
  {
    "Key": "SettingsIcon",
    "Value": "settings2.ico",
    "culture": "ar-SA"
  }
]
```

### Accessing in Code
Call the TLocalized.GetSpecial("SettingsIcon") function to get the value that is appropriate for the current culture.

<br/><br/><br/>

# XamlElements

### Set the detectable xaml element and property types

We can't know all the possible element types and properties to scan for, especially with third-party or custom controls. So you need to specify which elements can be detected during a scan.

This is stored in the `\Translator\XamlElement.json`. You can edit this file.

### `\Translator\XamlElement.json`

Note: When using a `.xaml` reference like `xmlns:ctWuiControls="using:CommunityToolkit.WinUI.Controls"` for `<ctWuiControls:SettingsExpander...>`, enter it as shown on the first element below.

```json
{
  "using:CommunityToolkit.WinUI.Controls:SettingsExpander": [
    "Header"
  ],
  "Button": [
    "Content"
  ],
  "TextBlock": [
    "Text"
  ],
  "CheckBox": [
    "Content"
  ],
  "ToggleSwitch": [
    "Header",
    "OffContent",
    "OnContent"
  ],
  "RadioButtons": [
    "Header"
  ],
  "RadioButton": [
    "Content"
  ],
  "ToggleButton": [
    "Content"
  ]
}
```






