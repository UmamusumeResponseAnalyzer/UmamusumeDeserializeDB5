# UmamusumeDeserializeDB5
https://github.com/EtherealAO/UmamusumeResponseAnalyzer 用事件数据生成工具，屎山上叠的代码，将就着看吧

运行 `dotnet build -c Release`，再进入 `bin/Release/net10.0` 执行 `./UmamusumeDeserializeDB5.exe`。生成器使用该目录下的 `correctedEventNames.txt` 和 `correctedTriggerNames.txt`，并将校正结果写回；运行前备份校正表及 `output`。日、台版数据分别生成到 `output/ja-JP`、`output/zh-TW`，ZIP 位于 `output`。

输入路径定义在 `Data.cs`（日、台版 `master.mdb`）和 `Generator/NewEvents.cs`（StoryDataExtractor 的 `Ext_Jp`）；`Ext_Tw` 路径由 `Data.UseTw()` 设置。故事提取数据的更新时间决定选项文本的覆盖范围。

技能进化条件的 `Type=0/None` 表示仅由服务器判定，不做本地预测。`Group` 取自主表 `skill_upgrade_condition.num`，同组条件取或，不同组取且。

`skill_planning.br` 为 Host 技能规划目录（格式 1），包含技能、费用门槛、原始发动条件、适性／效果标签、角色／剧本／继承进化和剧本名额。持有条件按日文或繁体中文主数据文本模板解析，未知持有条件记录为 `Unverified`；数值进度及门槛由报文提供。效果标签按 GameAssembly 的 `GetSkillEffectTag` 分类，不能用图标或速度属性标签代替速度效果。

仅生成技能规划目录，不运行事件生成：

```powershell
dotnet run -c Release -- --skill-planning <master.mdb路径> <输出目录>
```

输入以 SQLite 只读方式打开。普通日／台版生成流程也包含此文件；`skill_data.br` 等文件供其他现有调用方使用。配套 Host／SDK 版本为 `2026.11.1.2`。
