import {themes as prismThemes} from 'prism-react-renderer';

const config = {
  title: 'AsyncAPI.NET',
  tagline: 'Build, read, and extend AsyncAPI documents in .NET.',
  favicon: 'img/dotnet-logo.svg',
  url: 'https://bytebardorg.github.io',
  baseUrl: '/AsyncAPI.NET/',
  organizationName: 'ByteBardOrg',
  projectName: 'AsyncAPI.NET',
  onBrokenLinks: 'throw',
  markdown: {hooks: {onBrokenMarkdownLinks: 'warn'}},
  i18n: {defaultLocale: 'en', locales: ['en']},
  presets: [
    ['classic', {
      docs: {routeBasePath: 'docs', sidebarPath: './sidebars.js', editUrl: 'https://github.com/ByteBardOrg/AsyncAPI.NET/edit/vnext/website/'},
      blog: false,
      theme: {customCss: './src/css/custom.css'},
    }],
  ],
  themeConfig: {
    image: 'img/social-card.svg',
    navbar: {
      title: 'AsyncAPI.NET',
      logo: {alt: '.NET logo', src: 'img/dotnet-logo.svg'},
      items: [
        {type: 'doc', docId: 'guides/getting-started', label: 'Use the library', position: 'left'},
        {type: 'doc', docId: 'guides/custom-extension-parsers', label: 'Extend the library', position: 'left'},
        {to: '/docs/bindings', label: 'Bindings', position: 'left'},
        {href: 'https://github.com/ByteBardOrg/AsyncAPI.NET', label: 'GitHub', position: 'right'},
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {title: 'Explore', items: [
          {label: 'Use the library', to: '/docs/guides/getting-started'},
          {label: 'Extend the library', to: '/docs/guides/custom-extension-parsers'},
          {label: 'Bindings', to: '/docs/bindings'},
        ]},
        {title: 'Project', items: [{label: 'GitHub', href: 'https://github.com/ByteBardOrg/AsyncAPI.NET'}]},
      ],
      copyright: `Copyright © ${new Date().getFullYear()} ByteBardOrg.`,
    },
    prism: {theme: prismThemes.github, darkTheme: prismThemes.dracula, additionalLanguages: ['csharp']},
  },
};

export default config;
