**[Download it at VS Marketplace](https://marketplace.visualstudio.com/items?itemName=RamonFMendes.HtmlView)**

A Visual Studio extensions that provides to the HTML editor an area where it shows a preview of your HTML page.
Essentially it is a Chrome based browser inside Visual Studio showing your HTML page. Internally it uses on [CEFSharp](https://github.com/cefsharp/CefSharp).

![](https://ramonfmendes.gallerycdn.vsassets.io/extensions/ramonfmendes/htmlview/1.3/1482143199965/207798/1/screenshot.png)

# Features

- **CTRL+SHIFT+CLICK** any element to inspect it
- JS method to check if you viewing your HTML inside HtmlView: `HtmlView.active == true`
- Chrome DevTools (shortcut: F12)
- Console area shows any log output (errors or console.log() calls)

# How to use

- previewer by default is always OFF for all .HTML files
- you must enable it in a per-file basis; to do it add the following line at the start of your HTML and save the file:

```<!-- HtmlView:on -->```

- it is always ON for files named 'unittest.html'
- you can toggle the previewer by simply setting it to off (or just remove the first line) and save the file:

```<!-- HtmlView:off -->```

- you can make the console area always visible:

```<!-- HtmlView:on, console:on -->```