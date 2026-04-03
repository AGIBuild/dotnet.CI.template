import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'ChengYuan',
  description: 'An ABP-inspired modular .NET template family built around framework and application modules',
  base: '/dotnet.CI.template/',

  sitemap: {
    hostname: 'https://agibuild.github.io/dotnet.CI.template'
  },

  head: [
    ['meta', { name: 'theme-color', content: '#512bd4' }],
    ['meta', { property: 'og:type', content: 'website' }],
    ['meta', { property: 'og:title', content: 'ChengYuan' }],
    ['meta', { property: 'og:description', content: 'An ABP-inspired modular .NET template family built around framework and application modules' }],
    ['meta', { property: 'og:url', content: 'https://agibuild.github.io/dotnet.CI.template/' }],
    ['meta', { name: 'twitter:card', content: 'summary' }],
    ['meta', { name: 'twitter:title', content: 'ChengYuan' }],
    ['meta', { name: 'twitter:description', content: 'An ABP-inspired modular .NET template family built around framework and application modules' }]
  ],

  locales: {
    root: {
      label: 'English',
      lang: 'en',
      themeConfig: {
        nav: [
          { text: 'Guide', link: '/guide/introduction' },
          { text: 'Reference', link: '/reference/api' },
          { text: 'Contributing', link: '/contributing/development' }
        ],
        sidebar: {
          '/guide/': [
            {
              text: 'Guide',
              items: [
                { text: 'Introduction', link: '/guide/introduction' },
                { text: 'Architecture', link: '/guide/architecture' },
                { text: 'Getting Started', link: '/guide/getting-started' }
              ]
            }
          ],
          '/reference/': [
            {
              text: 'Reference',
              items: [
                { text: 'API', link: '/reference/api' }
              ]
            }
          ],
          '/contributing/': [
            {
              text: 'Contributing',
              items: [
                { text: 'Development', link: '/contributing/development' },
                { text: 'CI/CD', link: '/contributing/ci-cd' },
                { text: 'Releasing', link: '/contributing/releasing' }
              ]
            }
          ]
        }
      }
    },
    'zh-cn': {
      label: '简体中文',
      lang: 'zh-CN',
      themeConfig: {
        nav: [
          { text: '指南', link: '/zh-cn/guide/introduction' },
          { text: '参考', link: '/zh-cn/reference/api' },
          { text: '开发贡献', link: '/zh-cn/contributing/development' }
        ],
        sidebar: {
          '/zh-cn/guide/': [
            {
              text: '指南',
              items: [
                { text: '产品介绍', link: '/zh-cn/guide/introduction' },
                { text: '架构设计', link: '/zh-cn/guide/architecture' },
                { text: '快速开始', link: '/zh-cn/guide/getting-started' }
              ]
            }
          ],
          '/zh-cn/reference/': [
            {
              text: '参考',
              items: [
                { text: 'API', link: '/zh-cn/reference/api' }
              ]
            }
          ],
          '/zh-cn/contributing/': [
            {
              text: '开发贡献',
              items: [
                { text: '开发环境', link: '/zh-cn/contributing/development' },
                { text: 'CI/CD 流程', link: '/zh-cn/contributing/ci-cd' },
                { text: '发版指南', link: '/zh-cn/contributing/releasing' }
              ]
            }
          ]
        }
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
