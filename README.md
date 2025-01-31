# WinUITranslator
## Translate your WinappSDK WinUI Apps.

> [!IMPORTANT]
> - Requires Visual Studio 2022 with .Net 8 and WinAppSDK 1.6

- This Visual Studio 2022 solution contains a Translator app that translates your WinAppSDK WinUI project to the languages of your choice.  It will scan your project, create the necessary translations, and update your Resource.resw files.
- An OpenAI API translation function is included.  You can translate an app for under $5 USD.
- You can also create your own translation function to call whatever translation service you want, including local and free models!
- Run the included sample apps.  They have already been translated from en-US to de-DE, fr-FR and ar-SA.  You can modify these and re-translate them to see how it works.
- Explore the included static class TLocalized and updated Resource.resw files.
- You only need to include the TLocalized static class in your project.  It allows dynamic text translations, like for a 'Loading {0}, please wait...' status message.  It also allows the retrieval of non-translated resources, such as icon files, colors etc.
- No third party packages or libraries are used.
- Trimming is supported.
- You don't need to edit the .resw files.  Ever.
- All translation changes are diff-able.

> [!NOTE]
> You use the Translator app to translate your target app.

# Why Does this Exist?
I needed to translate several WinAppSDK Winui apps.  I tried some Nuget package tools, but realized that I really don't understand how the resources/translation/localization thing works.  I figured if I made my own tool that can meet the requirements listed below, I would understand.  I posted this as a public repository to share it, and see what ideas/insights other devs may have.

Now that AI is everywhere, I set out to use AI to do the translations, and it works really well.

> [!TIP]
> Take a close look at the packaged and unpackaged sample apps.  It helps clarify the obscure and somewhat undocumented process of setting the language and testing with RTL languages.

<br/><br/><br/>

Note: the following info is also displayed in the tabs in the Translator app.

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
> Translate it. Test the translations under different languages and understand how it works.<br/><br/>
> Then you'll know what you need to do to make your app localized. It will affect your decisions on layouts, text length, and spacing that are difficult to fix later.
> Starting this process of adding `x:Uid`'s, `.Get()`'s, and running the Translator app **after your app is already big is a pain.**
> I know this. Best to start translating on day 1.

