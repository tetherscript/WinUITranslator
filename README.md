# WinUITranslator
## Translate your WinappSDK Apps.

- This Visual Studeio 2022 solution contains a Translator app that translates your WinAppSDK WinUI project to the languages of your choice.  It will scan your project, create the necesary translations, and update your Resource.resw files.
- An OpenAI API translation function is included.  You can translate an app for under $5 USD.
- You can also create your own translation function to call whatever translation service you want, including local and free models!
- Run the included sample apps.  They have already been translated from en-US to de-DE, fr-FR and ar-SA.  You can modify these and re-translate them to see how it works.
- Explore the included static class TLocalized and Resource.resw files that are the only things that you deploy with your project.
- No third party packages or libraries are used.
- Trimming is supported.
- This is built using Visual Studio 2022 with .Net 8 and WinAppSDK 1.6.

---

# Why Does this Exist?
I needed to translate several WinAppSDK Winui apps.  I tried some Nuget package tools, but realized that I really don't understand how the resources/translation/localization thing works.  I figured if I made my own tool that can meet the requirements listed below, I would understand.  I posted this as a public repository to share it, and see what ideas/insights other devs may have.

Now that AI is everywhere, I set out to use AI to do the translations, and it works really well.  Really nice.

---

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

---

## Solution

1. In your app to be translated (the "target"), add a `\Translator` folder and some files. Adjust your `.xaml` elements and dynamic text as shown in the `x:Uid`'s and `TLocalized.Get()` tabs.
2. In this Translator app, set the target to that app, scan, then translate.
3. Build and run the target app. The translations are now visible.

---

## Try it with the Sample Apps

1. Run the packaged or unpackaged sample app in VS2022.
2. Run this Translator app in another instance of VS2022, or just run the deployed version from the start menu.
3. The sample app is ready to convert. Target, scan, and translate, then run the sample app to see the changes.

---

## Ideal Real-World Usage

1. Continue developing and debugging your app.
2. When you are ready to translate, run this Translator app, then run your app again to see the changes.
3. Tweak your app code, translate, tweak code, translate. Rinse. Repeat until it is fully translated.

---

## My Best Advice

If you are creating an app that will need to be translated, **you need to set it up and test the translation on day one of development.**  
- Put a Hello World button, a dynamic text message, and define several languages.
- Translate it. Test the translations under different languages.
- Understand how it works.

Then you'll know what you need to do to make your app localized. It will affect your decisions on layouts, text length, and spacing that are difficult to fix later.

Starting this process of adding `x:Uid`'s, `.Get()`'s, and running the Translator app **after your app is already big is a pain.**  
I know this. Best to start translating on day 1.


# The Target

You have an app project that you want to translate.  That's the Target.

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


# Scanning

- Searches `.xaml` files used by the target project and detects any elements that have a `x:Uid` and are defined in `XamlElements.json`. 
- It combines this with the items in `TLocalizedGets.json` and `Specials.json` and updates the `target/Strings/en-US/Resources.resw`. 
- You put a blank `Resources.resw` there earlier, and its data section has been replaced with the scan results. This `.resw` is the "truth" for what needs to be translated later.
- In that `Resources.resw` file, you'll see that the hint token (`@`, `@@`, `!`, `!!`) have been placed in the comments field. **Don't edit this file.** 
- No other `.resw` files have been touched. That will happen when you translate.
- The contents of `DetectedXamlElements.json` have also been replaced.

## Summary

When a scan is complete, you have found the things that need to be translated, but you have not actually translated anything yet.











