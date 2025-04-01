# VolvoWroclawConf2025

This repository showcases how to make a web-page that act as an application, a [Progressive Web App](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps/Guides/What_is_a_progressive_web_app) that can be installed on desktops and mobile devices and has capabilities comparable to a native applications.

This is targeted at young software developers who already know basics of programming: at least 1 modern programming language, a general idea of databases and maybe some HTML, now they want to make a useful or fun web / mobile application but are drowning in the abundance of technological choice, struggling to decide between multiple viable solutions for every problem. Experienced devs can also benefit if they are looking for fast app prototyping options.

The app is made with minimal resource overhead in mind (with one notable exception). The goal is to make a prototype as quickly as possible, skipping any non-essential step and taking every possible shortcut. This application is built for rapid deployment to modern cloud for less than 5$ per month.

## Progressive web apps

A Progressive Web App ( PWA ) is a web app that can be [installed on users device permanently](https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps/Guides/Making_PWAs_installable). It offers a lot additional functionality compared to regular web page, like the ability to work offline, but this project mostly uses PWA because it looks, feels and behaves like a native (i.e. mobile iOS or Android) application and because installation is required to receive push-notifications on some platforms.

PWAs have a major advantage over modern day native mobile apps - PWAs don't have to go through platform-dependant application stores. No need to wait for approval on every update, no need to follow arcane platform rules. The downside is - they are often 2-nd priority for the the company which owns the platform compared to the native aps. Nevertheless, PWA technology is mature and widespread these days, having the benefit of being free and company-neutral to it's advantage which lead to it's mass adoption.

![PWAs](./readme/PWAs.png)

A typical PWA is made with JavaScript/TypeScript and one of the leading [Single Page Application](https://developer.mozilla.org/en-US/docs/Glossary/SPA) frameworks like Angular / React / SolidJS / Svelte etc... However, modern web browsers have become full fledged application platforms in the last 15 years and one of their key new capabilities is [Web Assembly](https://webassembly.org/) or WASM. WASM is one of the latest aspirant on the arena of Virtual Machines (like JVM, CLR or LLVM), it is designed as a portable compilation target for programming languages and let's you create web applications and by extension PWAs with a variety of programming languages like Python, Rust, C# etc... JavaScript still remains "Lingua franca" of modern web, and is primary choice for PWAs, but making both UI and Server part in a single language in which you are well proficient has its own advantages. Due to this reason, I have gone away from "shortest path" rule here and decided to showcase a Blazor PWA made in C#. You will never be able to get rid of JS completely in a PWA, but WASM will allow you to limit it to technical glue-code and make business logic in your language of choice.

## Modern Cloud and development ecosystem 

Modern cloud is incredibly powerful and efficient. So efficient in fact, that Cloud providers are not just wiling to give you free credits for a trial but also give you considerable resources for free permanently. 

![AWS offering](./readme/AWS_offering.png)

![Azure offering](./readme/Azure_offering.png)

They give you Enterprise grade stuff for free and in return they get a highly motivated professional developer familiar with their technology - both sides win. If you look around, you will find that many PaaS providers have generous free tiers these days.

It is also for this reason, that companies like Microsoft and Jetbrains provide "Community" editions of their premier IDEs free of charge to non-commercial users (you just need to register a free account). For this project I recommend downloading [Visual Studio Community](https://visualstudio.microsoft.com/vs/community/), for which you will need an Microsoft Account [outlook.live.com](https://www.microsoft.com/en-us/microsoft-365/outlook/email-and-calendar-software-microsoft-outlook). This account will also be used to access [Azure Cloud](https://portal.azure.com/). 

Another account you will need is with [Google](https://accounts.google.com/) which you will use to access Firebase Cloud.  

## 
