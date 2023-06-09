# 在 .NET6.0 中实现 Leaf-segment 分布式 ID 生成系统

使用方式：
```
var idGenerator = IdGeneratorFactory.GetDefault("Server=127.0.0.1;Port=3306;Uid=root;Pwd=keikei;DataBase=netleaf;CharSet=utf8;allow zero datetime=true;Max Pool Size=10000;");
var idGenerator.Init();
var id = idGenerator.Generation();
```

详细信息： https://blog.csdn.net/ruiye99/article/details/131011405
