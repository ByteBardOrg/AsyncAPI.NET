import {createHighlighterCoreSync} from 'shiki/core';
import {createJavaScriptRegexEngine} from 'shiki/engine/javascript';
import csharp from '@shikijs/langs/csharp';
import json from '@shikijs/langs/json';
import shellscript from '@shikijs/langs/shellscript';
import yaml from '@shikijs/langs/yaml';
import darkPlus from '@shikijs/themes/dark-plus';
import lightPlus from '@shikijs/themes/light-plus';

// The Visual Studio Code themes, matching prismThemes.vsLight and prismThemes.vsDark in docusaurus.config.js,
// which style the rest of the code block.
const themes = {light: 'light-plus', dark: 'dark-plus'};

// Synchronous, so code blocks render the same on the server and in the browser.
const highlighter = createHighlighterCoreSync({
  themes: [lightPlus, darkPlus],
  langs: [csharp, json, shellscript, yaml],
  engine: createJavaScriptRegexEngine(),
});

/**
 * Tokenizes code into lines of tokens, each styled with both themes' colors as CSS variables
 * (--shiki-light and --shiki-dark), which custom.css applies for the current color mode.
 */
export function highlight(code, language) {
  const lang = highlighter.getLoadedLanguages().includes(language) ? language : 'text';
  return highlighter.codeToTokens(code, {lang, themes, defaultColor: false}).tokens;
}
