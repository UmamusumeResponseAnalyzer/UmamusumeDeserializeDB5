# UmamusumeDeserializeDB5
https://github.com/EtherealAO/UmamusumeResponseAnalyzer 用事件数据生成工具，屎山上叠的代码，将就着看吧

运行 `dotnet build -c Release`，再进入 `bin/Release/net10.0` 执行 `./UmamusumeDeserializeDB5.exe`。生成器使用该目录下的 `correctedEventNames.txt` 和 `correctedTriggerNames.txt`，并将校正结果写回；运行前备份校正表及 `output`。日、台版数据分别生成到 `output/ja-JP`、`output/zh-TW`，ZIP 位于 `output`。

输入路径定义在 `Data.cs`（日、台版 `master.mdb`）和 `Generator/NewEvents.cs`（StoryDataExtractor 的 `Ext_Jp`）；`Ext_Tw` 路径由 `Data.UseTw()` 设置。故事提取数据的更新时间决定选项文本的覆盖范围。

技能进化条件的 `Type=0/None` 表示仅由服务器判定，不做本地预测。`Group` 取自主表 `skill_upgrade_condition.num`，同组条件取或，不同组取且。
