// ============================================================
//  SceneSetupGuide.cs  –  Unity scene wiring documentation
// ============================================================
//
//  ┌─────────────────────────────────────────────────────────┐
//  │  SCENE HIERARCHY                                        │
//  └─────────────────────────────────────────────────────────┘
//
//  [Scene: TacoVsBurrito]
//  │
//  ├── _Managers  (empty GameObject, DontDestroyOnLoad)
//  │   ├── GameManager          → GameManager.cs
//  │   │     cardDatabase       → assign CardDatabase SO (optional)
//  │   │     numberOfPlayers    → 4
//  │   │     startingHandSize   → 5
//  │   └── AudioManager         → AudioManager.cs + AudioSource
//  │         (assign SFX clips in Inspector)
//  │
//  ├── Canvas_Setup  (Screen Space Overlay)
//  │   └── SetupPanel
//  │       ├── PlayerSetupUI    → PlayerSetupUI.cs
//  │       │     rowsParent           → Content (ScrollView)
//  │       │     playerRowPrefab      → PlayerRow prefab
//  │       │     addPlayerButton      → Button "+"
//  │       │     removePlayerButton   → Button "-"
//  │       │     startButton          → Button "Start Game"
//  │       │     playerCountLabel     → TMP text
//  │       │     foodieExpansionToggle→ Toggle (unlocks 5-8 players)
//  │       │     validationLabel      → TMP text (warnings)
//  │       └── ScrollView
//  │           └── Content            ← rowsParent
//  │
//  ├── Canvas_Game  (Screen Space Overlay, hidden until game starts)
//  │   └── GameHUD              → GameHUD.cs
//  │       ├── TurnInfoBar
//  │       │   ├── Text_Turn          ← turnLabel
//  │       │   ├── Text_DrawPile      ← drawPileLabel
//  │       │   └── Text_Status        ← statusLabel
//  │       │
//  │       ├── Panel_Hand
//  │       │   ├── Text_HandOwner     ← handOwnerLabel
//  │       │   └── HorizLayout        ← handContainer
//  │       │
//  │       ├── Panel_Meals
//  │       │   └── HorizLayout        ← mealsContainer
//  │       │       (MealPanelUI prefabs instantiated per player)
//  │       │
//  │       ├── Panel_Log
//  │       │   └── ScrollRect
//  │       │       └── Text_Log       ← logText
//  │       │
//  │       ├── Overlay_NoBueno        ← noBuenoPanel (inactive)
//  │       │   ├── Text_Prompt        ← noBuenoPromptLabel
//  │       │   └── Button_NoBueno     ← noBuenoButton
//  │       │
//  │       ├── Overlay_Target         ← targetOverlay (inactive)
//  │       │   ├── Text_Prompt        ← targetPromptLabel
//  │       │   └── VertLayout         ← targetButtonsParent
//  │       │
//  │       ├── Overlay_MealCard       ← mealCardOverlay (inactive)
//  │       │   ├── Text_Prompt        ← mealCardPromptLabel
//  │       │   └── HorizLayout        ← mealCardButtonsParent
//  │       │
//  │       ├── Overlay_Trash          ← trashOverlay (inactive)
//  │       │   ├── Text_Prompt        ← trashPromptLabel
//  │       │   └── HorizLayout        ← trashButtonsParent
//  │       │
//  │       └── Overlay_DestMeal       ← destMealOverlay (inactive)
//  │           ├── Text_Prompt        ← destMealPromptLabel
//  │           └── VertLayout         ← destMealButtonsParent
//  │
//  └── Canvas_Scoreboard  (Screen Space Overlay, hidden initially)
//      └── ScoreboardUI    → ScoreboardUI.cs
//          ├── Text_WinnerName        ← winnerNameLabel
//          ├── Text_WinnerScore       ← winnerScoreLabel
//          ├── Text_WinnerMeal        ← winnerMealLabel
//          ├── Image_Banner           ← winnerBanner
//          ├── VertLayout_Rows        ← rowsParent
//          ├── Button_PlayAgain
//          └── Button_Quit
//
//  ┌─────────────────────────────────────────────────────────┐
//  │  PREFABS REQUIRED                                       │
//  └─────────────────────────────────────────────────────────┘
//
//  1. PlayerRow
//     Component: PlayerRowUI
//     Children:
//       Text_Index      (TMP)        → indexLabel
//       Input_Name      (TMP Input)  → nameInput
//       Btn_Taco        (Button)     → tacoButton
//       Btn_Burrito     (Button)     → burritoButton
//       Btn_Human       (Button)     → humanButton
//       Btn_AI          (Button)     → aiButton
//       Dropdown_Diff   (TMP Drop)   → aiDifficultyDropdown
//
//  2. CardButton  (hand cards)
//     Components: Button, Image
//     Children:
//       Text_Card  (TMP) → shows name + point label
//     Recommended size: 110×150px
//
//  3. MealPanel
//     Component: MealPanelUI
//     Children:
//       Text_PlayerName  (TMP)           → playerNameLabel
//       Text_Score       (TMP)           → scoreLabel
//       Text_Multiplier  (TMP)           → multiplierLabel
//       VertLayout_Cards (Transform)     → cardListParent
//     Note: MealCardChip prefab (small Image + TMP) assigned to mealCardChipPrefab
//
//  4. TargetButton  (player selection)
//     Components: Button
//     Children:
//       Text_Info (TMP) → name + meal + score
//
//  5. ScoreRow  (scoreboard)
//     Components: HorizLayoutGroup, Image
//     Children (4× TMP): rank | name | breakdown | total
//
//  ┌─────────────────────────────────────────────────────────┐
//  │  CARD DATABASE SCRIPTABLE OBJECT                        │
//  └─────────────────────────────────────────────────────────┘
//
//  Right-click Project → Create → TacoVsBurrito → CardDatabase
//  Leave allCardAssets empty → GameManager uses BuildDefaultDeck()
//  which generates all 56 cards at runtime.
//
//  Only populate allCardAssets if you want custom sprites per card.
//
//  ┌─────────────────────────────────────────────────────────┐
//  │  ADDING CARDS                                           │
//  └─────────────────────────────────────────────────────────┘
//
//  1. Add new CardData entries in CardDatabase.BuildDefaultDeck()
//  2. For new action types: add enum value in CardDefinitions.cs,
//     add resolve method in ActionResolver.cs,
//     add switch case in GameManager.ResolveActionCard()
//  3. Add AudioClip in AudioManager.cs if needed
//
//  ┌─────────────────────────────────────────────────────────┐
//  │  QUICK START (test without UI)                          │
//  └─────────────────────────────────────────────────────────┘
//
//  var data = new List<PlayerSetupData> {
//    new() { playerName="Alice", mealType=MealType.Taco,    playerType=PlayerType.Human },
//    new() { playerName="Bob",   mealType=MealType.Burrito, playerType=PlayerType.AI   },
//  };
//  GameManager.Instance.StartGame(data);
//
//  ┌─────────────────────────────────────────────────────────┐
//  │  PACKAGES                                               │
//  └─────────────────────────────────────────────────────────┘
//  Only TextMeshPro required (com.unity.textmeshpro).
//  No external packages needed.

namespace TacoVsBurrito
{
    /// <summary>Documentation-only class. No runtime logic.</summary>
    public static class SceneSetupGuide { }
}