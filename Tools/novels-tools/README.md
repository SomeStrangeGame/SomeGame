# novels-content

Единственный CLI для контентных Unity-проектов:

```bash
Tools/novels-tools/novels-content doctor
Tools/novels-tools/novels-content validate <catalog|story-id|all>
Tools/novels-tools/novels-content build <catalog|story-id|all> <editor|android|ios>
Tools/novels-tools/novels-content publish <destination-directory>
```

`build` автоматически компонует результат для Game. Проекты обрабатываются
последовательно, поэтому команда безопасна для больших наборов контента.
