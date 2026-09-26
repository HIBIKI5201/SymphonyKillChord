# Behavior Graphについて

- id: 3547c2c6-cc02-807d-9f32-d830fe34490f
- path: Symphony Kill Chord / システム概要 / Behavior Graphについて
- last_edited: 2026-05-04T07:07:05.545Z

- 概要: Behavior Graphの概要説明と、独自ノードの作成方法の説明を行う。
- カテゴリー: 開発用


## 説明
Behavior Graphの概要説明と、独自ノードの作成方法の説明を行う。

### 参考資料
公式ドキュメント
‣
個人記事
‣
‣

## 詳細

### 概要
  　UnityのBehavior Graphは、「Behavior TreeとState Machineの融合体」的なイメージ。一つのノードに「開始」「実行中」「成功」「失敗」の状態がある上、開始時と終了時に呼び出されるメソッドも用意されている。
　なお、Blackboardも存在しているが、これは開発者自身色々設定する必要があり、手数がかかる代わりに自由度が高いことが特徴。

### 開発思想
  - 条件と行動は細かく分解してノードを作成する。
「再利用」「組み立て」のイメージでノードを使用する。
  - ノード内に複雑な計算処理をしない。
行うのは情報取得、判定、指示、というシンプルなことのみ。
  - BlackBoardにはView層にあるファサードを設定する。ノードがファサードから情報を取得したり、対象に指示を出したりする。
‣
  - State Machineの要素もあるため、「このノードがどう終了するか」「成功／失敗したらどこに行くのか」を意識する。
  - 行動の優先度と、行動実行中に中断される可能性を意識する。

### ノードについて
  Unity Behaviorのノードは主に下記種類となる：
| **種類** | **説明・概要** | **使用する場面** |
|---|---|---|
| Action | 具体的な動作を実行する | 移動、攻撃、アニメーション再生などの指示 |
| Condition | 条件を評価しtrue／falseを返す | 射程入り時や、攻撃可否などの判定 |
| Flow/Sequencing | ノードの実行順番、ノード間の遷移を制御する Behavior TreeのSelectorはここに | 一定の順番で行動を行う、状態による行動分岐、状態変わった時に行動を中断など |
| SubGraph | 別のBehavior Graphを呼び出す | 同じロジックを共通化・再利用など |
| Event | 外部イベントを受け取る | 例えばダメージ受ける時に怯むアニメーションを再生するActionを実行、など |
  　現状は、主にActionとConditionノードを独自作成して、packageで提供されたFlow系ノードに合わせて敵の行動AIを作成している。

### Blackboardについて
  　BlackboardはGraphにある全ノードがアクセスできる、変数を置く空間である。
  　このプロジェクトでは、Blackboardに設定するものはファサード（Facade）とする。ファサードは、敵や環境の情報、及び行動を制御するメソッドをシンプルな形にして外に出して、ノードにアクセスさせる役割となる。
  　Blackboard変数には、型の制限がある。概して、設定できるのは基本的な値型（int, string, enumなど）と、Unityフレームワークがサポートする型（GameObject, Transform, Animator, MonoBehaviourなど）。
　よって、Blackboardにあるファサードは、View層に存在する、MonoBehaviourを継承したクラスとされている。
  [header_4] Blackboard変数の設定方法
    - Graph画面でBlackboardの右上にある「+」をクリックして、適宜なデータ型を選択する。
画像検索を利用して作成済みファサードを選択している。
      [image: image.png] attachment:8fa3eb12-56ef-4bb3-a9ec-b9b7d72d41bd:image.png
    - Blackboard変数を設定した後、Graphを使用するGameObjectを選択して、そのInspectorのBehavior AgentコンポーネントのBlackboardに設定することができる。
      [image: image.png] attachment:a6c8910e-4d8f-40ba-ae06-fa1ca3ff88a2:image.png
    - Graph画面にて、Blackboard変数の「Exposed」属性を設定できる。これをオフにした場合、GameObjectのInspectorではこの変数が非表示なる。ドラッグ＆ドロップではなく、コードでBlackboard変数を設定する場合、この属性を調整する。
    [image: image.png] attachment:934a136d-d70b-40b8-a492-ddd52f5a61f7:image.png
    [image: image.png] attachment:d1e7fc5d-159f-43d4-9d19-d7ec958c60ab:image.png
    
    - Blackboard変数の「Shared」属性をオンにすると、変数このGraphを使っているすべてのGameObject間の共通変数になる。周囲環境の情報や、プレイヤー情報の参照はSharedに設定することが想定されている。
    

### 独自ノードの作成

#### Actionノード
    - Graphの空いている箇所に右クリックして、Create New → [Action]をクリックする
      [image: image.png] attachment:ae1e4d5f-ab18-4bb0-9723-00a46af059a9:image.png
    - ノード名とカテゴリを設定してNextをクリックする
ノード名は、後に「Action」が付けられて、スクリプトのクラス名になる。
カテゴリは、Scriptable ObjectのmenuNameのように、メニュー階層を作るための情報。
      [image: image.png] attachment:0af22703-bdac-4c90-9d0f-a9d2b7cb5c77:image.png
    - ノードの説明文を設定してCreateをクリックする
ここで、半角スペース区切りでノードの説明、及びパラメータを設定する。
単語ごとに、単純文字か、特定型のパラメータか設定できる。が、型には制限があり、基本的な値型とUnityフレームワークがサポートする型しか設定できない。
※日本語も使えるが、パラメータで日本語を使うと、生成されたコードの変数名も日本語になってしまうので、日本語の使用は単純文字（Regular Text）だけにしましょう。
      [image: image.png] attachment:80af39f7-4a48-4a88-ae4d-a425f0fc7db4:image.png
    - 生成されたノードに、BlackboardからDrag & Dropして設定する
      [image: image.png] attachment:3ace962c-1f9a-4f0e-b997-e37eba2a06bf:image.png
      [image: image.png] attachment:85955d9c-d1c2-4fcf-9b6e-8c03fd245d5e:image.png
    - 生成されたコードの「story」、「category」にて、前述の説明文やカテゴリなどを変えることができます。
      [image: image.png] attachment:3807ea87-bd80-44f7-9b18-8bc8cc4b428e:image.png

#### Conditionノード
    　Conditionノードは、あらかじめGraphに「条件が付いているノード」配置してから、追加することができる。
　配置済みのノードを選択して、Inspectorにある「Assign Condition」をクリックして、「Create new Condition」を選択すると、Conditionノードを作成できる。
　後の手順は前述のノードとほぼ同じなので省略とする。
    [image: image.png] attachment:a08fd7f5-5c6b-40d2-af96-39f62c5e8b0f:image.png
    [image: image.png] attachment:0e7e2bd0-c85d-409d-a807-9bbfc78a4bce:image.png

#### ノードのスクリプト
    ActionノードとConditionノードは、主役となるメソッドが異なる。
    [header_4] Actionノード
      ActionノードのOnStart()とOnUpdate()メソッドには戻り値がある。
Status.Successが成功、Status.Failueが失敗、Status.Runningが実行中を表す。
成功と失敗の場合、Graphの作りに従って次に行くべきノードに遷移する。実行中の場合、このノードに止まって、毎フレームOnUpdate()メソッドが呼び出される。
      - OnStart()
このノードに遷移した時呼び出されるメソッド。
主に前処理と、事前の状態確認を行う。
      - OnUpdate()
OnStart()メソッドがStatus.Runningを返却すると、次にOnUpdate()が毎フレーム呼び出される。
主に毎フレームの処理と判定を担当する。
      - OnEnd()
ノード終了時に呼び出されるメソッド。
主に後処理を担当する。
    [header_4] Conditionノード
      Conditionノードは、IsTrue()メソッドが主役。このメソッドで情報を取得して、判定するのが一般的。メソッド返却値がConditionノードの結果となる。
なお、返却値のないOnStart()とOnEnd()を持っている。これらのメソッドは前処理と後処理を行う。

### 作成済みノード一覧
| **ファイル名** | **種類** | **概要** |
|---|---|---|
| AttackTargetAction | Action | プレイヤーに攻撃する |
| GetStunnedAction | Action | クリティカルヒットされた時の硬直アニメーションを再生する |
| MoveToAttackAction | Action | 攻撃を行える位置になるまで移動する |
| StopMovingAction | Action | 移動を停止する |
| IsTargetInAttackRangeCondition | Condition | プレイヤーが自分の射程範囲内に居るか |
| IsAimSightClearCondition | Condition | 自分とプレイヤーの間に障害物がないか |
| IsAttackingCondition | Condition | 攻撃を実行中か |
| IsStunnedCondition | Condition | 硬直中か |

### 作成済みのファサード一覧
| **ファイル名** | **説明** |
|---|---|
| EnemyMovementAIFacade | 敵の移動に関する情報、指示メソッドを持つ |
| EnemyBattleAIFacade | 敵の戦闘に関する情報、指示メソッドを持つ |
| EnemyStateFacade | 敵の状態に関する情報、指示メソッドを持つ |
| EnemySharedFacade | Shared属性がオン。敵全員に共通する情報を持つ ※まだ未使用 |
