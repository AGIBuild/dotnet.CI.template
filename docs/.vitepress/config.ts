import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'dotnet.CI.template',
  description: 'Production-ready .NET CI/CD template',
  base: '/dotnet.CI.template/',

  locales: {
    root: {
      label: 'English',
      lang: 'en',
      themeConfig: {
        nav: [{ text: 'Guide', link: '/quick-start-release' }],
        sidebar: [
          {
            text: 'Guide',
            items: [
              { text: 'Quick Start Release', link: '/quick-start-release' },
              { text: 'GitHub Workflows Guide', link: '/github-workflows-guide' }
            ]
          }
        ]
      }
    },
    'zh-cn': {
      label: '简体中文',
      lang: 'zh-CN',
      themeConfig: {
        nav: [{ text: '指南', link: '/zh-cn/quick-start-release' }],
        sidebar: [
          {
            text: '指南',
            items: [
              { text: '快速发版', link: '/zh-cn/quick-start-release' },
              { text: 'Workflows 指南', link: '/zh-cn/github-workflows-guide' }
            ]
          }
        ]
      }
    }
  },

  themeConfig: {
    socialLinks: [
      { icon: 'github', link: 'https://github.com/AGIBuild/dotnet.CI.template' }
    ],
    search: { provider: 'local' }
  }
})
