# WinUITranslator
### Translate your WinappSDK WinUI Apps using OpenAI, a local model or any other method you can dream up.
<br/>

> [!TIP]
> View the Wiki for the full documentation.

> [!IMPORTANT]
> - Requires Visual Studio 2022 with .Net 8 and WinAppSDK 1.6

- This Visual Studio 2022 solution contains a Translator app that translates your WinAppSDK WinUI project to the languages of your choice.  It will scan your project, create the necessary translations, and update your Resource.resw files.
- An OpenAI profile is included.  You can translate an app for under $5 USD.
- Local LMStudio profiles are included for deepseek-r1-distill-llama-8b, llama-3.2-1b-instruct, meta-llama-3.1-8b-instruct and qwen2.5-7b-instruct-1m models.  Runs on a 8gb GPU, and for free!  You can add new models by copying and modifiying an existing profile.
- Try the scan and translated workflow with the included sample apps.  It will just retrieve the already-translated cached translations and give you an idea of the workflow.
- You can also code your own profile to call whatever translation service you want, including local and free models!
- Run the included sample apps.  They have already been translated from en-US to de-DE, fr-FR and ar-SA.  You can modify these and re-translate them to see how it works.
- Explore the included static class TLocalized and updated Resource.resw files.
- You only need to include the TLocalized static class in your project.  It allows dynamic text translations, like for a 'Loading {0}, please wait...' status message.  It also allows the retrieval of non-translated resources, such as icon files, colors etc.
- Your deployables do not require any third party packages or libraries.  All the Translator-required code is contained in the included ```TLocalized.cs``` which is very simple, short and reviewable.  In fact, if you don't use any dynamic translations, you don't even need to include that.
- All translation settings and results are json/text files so you can include them in version control.
- Trimming is supported.
- Supports non-translateable resource items such as different icon and colors per culture.
- You don't need to edit the .resw files.  Ever.
<br/>

> [!NOTE]
> You use the Translator app to translate your target app.  Here it is using OpenAI GPT4-o-mini to translate the included ```Sample-Packaged``` app from en-US to ar-SA, de-DE, and fr-FR.  It has found that the en-US 'Connected' requires translation to ar-SA (Arabic), de-DE (Germani) and fr-FR (French).  The translation result for de-DE is expanded to show the OpenAI API response, including reasoning.  These translations are saved to a local cache file so that you don't need to re-translate these again.

![translate1](https://github.com/user-attachments/assets/5197a496-a259-43a9-a58f-f4897a228e40)

## Why Does this Exist?
I needed to translate several WinAppSDK Winui apps.  I tried some Nuget package tools, but realized that I really don't understand how the resources/translation/localization thing works.  I figured if I made my own tool that can meet the requirements listed below, I would understand.  I posted this as a public repository to share it, and see what ideas/insights other devs may have.  Now that AI is everywhere, I set out to use AI to do the translations, and it works really well.

This has greatly improved my dev-translate-test workflow.

> [!TIP]
> Take a close look at the packaged and unpackaged sample apps.  It helps clarify the obscure and somewhat undocumented process of setting the language and testing with RTL languages.
