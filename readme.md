-------------------------------------目录规范-------------------------------------
Art/                          # 存放游戏的全部资源，未被引用的资源将不会被打包
├── Animation/
├── Audio/
├── Font/
├── Material/
├── Model/
├── Shader/
├── Texture/
└── Video/

Resources/               # 存放少量必要的需要在运行时动态加载的资源，这些资源无论是否被引用都会参与构建
├── Animations/
├── Audio/
├── Data/
├── Materials/
├── Prefabs/
├── Textures/
└── Videos/

StreamingAssets/     # 外部大型文件
├── Audio/
├── Bundles/
├── Data/
├── Textures/
└── Videos/

· 原则上所有游戏资源都应该存放在Art目录下，并使用Addressables系统加载。只有需要频繁代码动态加载且不希望使用Addressables的资源需要放在Resources中