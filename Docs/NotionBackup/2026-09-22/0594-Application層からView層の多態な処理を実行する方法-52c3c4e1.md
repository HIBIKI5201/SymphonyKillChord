# Application層からView層の多態な処理を実行する方法

- id: 3537c2c6-cc02-8069-afff-c85e52c3c4e1
- path: Symphony Kill Chord / システム概要 / 設計思想 / Application層からView層の多態な処理を実行する方法
- last_edited: 2026-05-01T13:05:07.231Z

Application
```C#
public interface IViewAction
{
    void Execute(int id);
}

public class SkillUseCase
{
    private readonly IViewAction _action;

    public SkillUseCase(IViewAction action)
    {
        _action = action;
    }

    public void Execute(int id)
    {
        _action.Execute(id);
    }
}
```
Adapter
```C#
public interface ISkillVisual
{
		int Id { get; }
    void Execute();
}

public class SkillController : IViewAction
{
    private readonly Dictionary<int,ISkillVisual> _visuals;

    public SkillController(ISkillVisual[] visuals)
    {
		    _visuals = new Dictionary<int, ISkillVisual>();
		    for (int i = 0; i < visuals.Length; i++)
		    {
				    _visuals.Add(visuals[i].Id, visuals[i]);
		    }
    }

    public void Execute(int id)
    {
        _visuals[id].Execute();
    }
}
```
View
```C#
public class SkillView : MonoBehaviour, ISkillVisual
{
		public int Id => id;
		
		[SerializeField] private int id; 
		
    public void Execute()
    {
        //実際のViewで起こる演出など
    }
}
```
Composition
```C#
public class SkillInitializer : MonoBehaviour
{
		private SkillUseCase skillUseCase;
		
		public void Initialize(ISkillVisual[] skills)
		{
				SkillController controller = new (skills);
				skillUseCase = new (action);
		}
}
```
