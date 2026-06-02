using Android.Content.PM;
using Android.Graphics;
using Android.Util;
using Android.Views;
using ShardsCounter.Models;
using ShardsCounter.Services;
using GameState = ShardsCounter.Models.GameState;

namespace ShardsCounter
{
    [Activity(Label = "@string/app_name", MainLauncher = true, ScreenOrientation = ScreenOrientation.Landscape, Theme = "@android:style/Theme.Material.NoActionBar.Fullscreen")]
    public class MainActivity : Activity
    {
        private readonly GameService _gameService = new();
        private GameStateStore? _gameStateStore;
        private GameState? _gameState;
        private LinearLayout? _identitySelectionLayout;
        private LinearLayout? _gameLayout;
        private LinearLayout? _player1IdentityButtons;
        private LinearLayout? _player2IdentityButtons;
        private LinearLayout? _player1Row;
        private LinearLayout? _player2Row;
        private TextView? _player1IdentityText;
        private TextView? _player2IdentityText;
        private View? _player1HealthControl;
        private View? _player1MasteryControl;
        private View? _player2HealthControl;
        private View? _player2MasteryControl;
        private LinearLayout? _turnControlsLayout;
        private View? _crystalsControl;
        private View? _damageControl;
        private Button? _newGameButton;
        private Button? _buyMasteryButton;
        private Button? _endTurnButton;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window?.AddFlags(WindowManagerFlags.KeepScreenOn | WindowManagerFlags.Fullscreen);
            ActionBar?.Hide();
            HideSystemUi();
            SetContentView(Resource.Layout.activity_main);

            _gameStateStore = new GameStateStore(this, _gameService);
            _gameState = _gameStateStore.Load();

            FindViews();
            CreateIdentityButtons();
            WireControls();
            Render();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            if (hasFocus)
            {
                HideSystemUi();
            }
        }

        protected override void OnPause()
        {
            SaveState();
            base.OnPause();
        }

        private void FindViews()
        {
            _identitySelectionLayout = FindViewById<LinearLayout>(Resource.Id.identitySelectionLayout);
            _gameLayout = FindViewById<LinearLayout>(Resource.Id.gameLayout);
            _player1IdentityButtons = FindViewById<LinearLayout>(Resource.Id.player1IdentityButtons);
            _player2IdentityButtons = FindViewById<LinearLayout>(Resource.Id.player2IdentityButtons);
            _player1Row = FindViewById<LinearLayout>(Resource.Id.player1Row);
            _player2Row = FindViewById<LinearLayout>(Resource.Id.player2Row);
            _player1IdentityText = FindViewById<TextView>(Resource.Id.player1IdentityText);
            _player2IdentityText = FindViewById<TextView>(Resource.Id.player2IdentityText);
            _player1HealthControl = FindViewById(Resource.Id.player1HealthControl);
            _player1MasteryControl = FindViewById(Resource.Id.player1MasteryControl);
            _player2HealthControl = FindViewById(Resource.Id.player2HealthControl);
            _player2MasteryControl = FindViewById(Resource.Id.player2MasteryControl);
            _turnControlsLayout = FindViewById<LinearLayout>(Resource.Id.turnControlsLayout);
            _crystalsControl = FindViewById(Resource.Id.crystalsControl);
            _damageControl = FindViewById(Resource.Id.damageControl);
            _newGameButton = FindViewById<Button>(Resource.Id.newGameButton);
            _buyMasteryButton = FindViewById<Button>(Resource.Id.buyMasteryButton);
            _endTurnButton = FindViewById<Button>(Resource.Id.endTurnButton);

            _player1HealthControl?.SetBackgroundColor(ParseColor("#2E7D32"));
            _player2HealthControl?.SetBackgroundColor(ParseColor("#2E7D32"));
            _player1MasteryControl?.SetBackgroundColor(ParseColor("#FDD835"));
            _player2MasteryControl?.SetBackgroundColor(ParseColor("#FDD835"));
            _crystalsControl?.SetBackgroundColor(ParseColor("#81D4FA"));
            _damageControl?.SetBackgroundColor(ParseColor("#C62828"));

            SetValueIcon(_player1HealthControl, Resource.Drawable.value_hp);
            SetValueIcon(_player2HealthControl, Resource.Drawable.value_hp);
            SetValueIcon(_player1MasteryControl, Resource.Drawable.value_mastery);
            SetValueIcon(_player2MasteryControl, Resource.Drawable.value_mastery);
            SetValueIcon(_crystalsControl, Resource.Drawable.value_crystals);
            SetValueIcon(_damageControl, Resource.Drawable.value_damage);

            SetValueTextSize(_crystalsControl, 120);
            SetValueTextSize(_damageControl, 120);
        }

        private void HideSystemUi()
        {
            if (Window?.DecorView is null)
            {
                return;
            }

            Window.DecorView.SystemUiFlags =
                SystemUiFlags.ImmersiveSticky |
                SystemUiFlags.Fullscreen |
                SystemUiFlags.HideNavigation |
                SystemUiFlags.LayoutFullscreen |
                SystemUiFlags.LayoutHideNavigation |
                SystemUiFlags.LayoutStable;
        }

        private void CreateIdentityButtons()
        {
            _player1IdentityButtons?.RemoveAllViews();
            _player2IdentityButtons?.RemoveAllViews();

            foreach (var identity in Services.GameService.Identities)
            {
                _player1IdentityButtons?.AddView(CreateIdentityButton(identity, PlayerSlot.Player1));
                _player2IdentityButtons?.AddView(CreateIdentityButton(identity, PlayerSlot.Player2));
            }
        }

        private Button CreateIdentityButton(GameIdentity identity, PlayerSlot slot)
        {
            var button = new Button(this)
            {
                Text = identity.Name,
                TextSize = 16
            };
            button.SetAllCaps(false);
            button.SetTextColor(Color.White);
            button.SetBackgroundColor(ParseColor(identity.Color));
            var layoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1);
            layoutParameters.SetMargins(4, 8, 4, 8);
            button.LayoutParameters = layoutParameters;
            button.Click += (_, _) => SelectIdentity(slot, identity.Name);
            return button;
        }

        private void WireControls()
        {
            WireValueControl(_player1HealthControl, () => _gameState?.Player1.HealthPoints ?? 0, delta => ChangeState(() => _gameService.AdjustHealth(State, PlayerSlot.Player1, delta)));
            WireValueControl(_player1MasteryControl, () => _gameState?.Player1.Mastery ?? 0, delta => ChangeState(() => _gameService.AdjustMastery(State, PlayerSlot.Player1, delta)));
            WireValueControl(_player2HealthControl, () => _gameState?.Player2.HealthPoints ?? 0, delta => ChangeState(() => _gameService.AdjustHealth(State, PlayerSlot.Player2, delta)));
            WireValueControl(_player2MasteryControl, () => _gameState?.Player2.Mastery ?? 0, delta => ChangeState(() => _gameService.AdjustMastery(State, PlayerSlot.Player2, delta)));
            WireValueControl(_crystalsControl, () => _gameState?.CurrentTurnCrystals ?? 0, delta => ChangeState(() => _gameService.AdjustCrystals(State, delta)));
            WireValueControl(_damageControl, () => _gameState?.CurrentTurnDamage ?? 0, delta => ChangeState(() => _gameService.AdjustDamage(State, delta)));

            if (_newGameButton is not null)
            {
                _newGameButton.Click += (_, _) => ShowNewGameConfirmation();
            }

            if (_buyMasteryButton is not null)
            {
                _buyMasteryButton.Click += (_, _) => BuyMastery();
            }

            if (_endTurnButton is not null)
            {
                _endTurnButton.Click += (_, _) => EndTurn();
            }
        }

        private static void WireValueControl(View? control, Func<int> getValue, Action<int> adjustValue)
        {
            if (control is null)
            {
                return;
            }

            control.FindViewById<Button>(Resource.Id.decreaseOneButton)!.Click += (_, _) => adjustValue(-1);
            control.FindViewById<Button>(Resource.Id.increaseOneButton)!.Click += (_, _) => adjustValue(1);
            control.FindViewById<Button>(Resource.Id.decreaseFiveButton)!.Click += (_, _) => adjustValue(-5);
            control.FindViewById<Button>(Resource.Id.increaseFiveButton)!.Click += (_, _) => adjustValue(5);
            control.FindViewById<TextView>(Resource.Id.controlValueText)!.Text = getValue().ToString();
        }

        private void SelectIdentity(PlayerSlot slot, string identityName)
        {
            _gameService.SelectIdentity(State, slot, identityName);

            if (!string.IsNullOrWhiteSpace(State.PendingPlayer1IdentityName) && !string.IsNullOrWhiteSpace(State.PendingPlayer2IdentityName))
            {
                if (!_gameService.TryStartGame(State, out var errorMessage))
                {
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        ShowMessage(errorMessage);
                    }
                }
            }

            SaveState();
            Render();
        }

        private void ChangeState(Action change)
        {
            change();
            SaveState();
            Render();
        }

        private void BuyMastery()
        {
            if (_gameService.BuyMastery(State))
            {
                SaveState();
                Render();
            }
        }

        private void EndTurn()
        {
            var winner = _gameService.EndTurn(State);
            SaveState();
            Render();

            if (winner is not null)
            {
                var winnerName = _gameService.GetPlayer(State, winner.Value).IdentityName ?? winner.Value.ToString();
                new AlertDialog.Builder(this)
                    .SetMessage($"Player {winnerName} won")
                    .SetPositiveButton(Resource.String.ok, (_, _) =>
                    {
                        _gameService.MoveToIdentitySelection(State);
                        SaveState();
                        Render();
                    })
                    .Show();
            }
        }

        private void ShowNewGameConfirmation()
        {
            new AlertDialog.Builder(this)
                .SetTitle(Resource.String.confirm_new_game_title)
                .SetMessage(Resource.String.confirm_new_game_message)
                .SetPositiveButton(Resource.String.yes, (_, _) =>
                {
                    _gameService.MoveToIdentitySelection(State);
                    SaveState();
                    Render();
                })
                .SetNegativeButton(Resource.String.no, (_, _) => { })
                .Show();
        }

        private void Render()
        {
            var state = State;
            var isIdentitySelection = state.Phase == GamePhase.IdentitySelection;

            if (_identitySelectionLayout is not null)
            {
                _identitySelectionLayout.Visibility = isIdentitySelection ? ViewStates.Visible : ViewStates.Gone;
            }

            if (_gameLayout is not null)
            {
                _gameLayout.Visibility = isIdentitySelection ? ViewStates.Gone : ViewStates.Visible;
            }

            RenderIdentitySelection();
            RenderGame();
        }

        private void RenderIdentitySelection()
        {
            UpdateIdentityButtons(_player1IdentityButtons, State.PendingPlayer1IdentityName);
            UpdateIdentityButtons(_player2IdentityButtons, State.PendingPlayer2IdentityName);
        }

        private void RenderGame()
        {
            RenderPlayer(PlayerSlot.Player1, _player1Row, _player1IdentityText, _player1HealthControl, _player1MasteryControl);
            RenderPlayer(PlayerSlot.Player2, _player2Row, _player2IdentityText, _player2HealthControl, _player2MasteryControl);

            SetValueControl(_crystalsControl, State.CurrentTurnCrystals);
            SetValueControl(_damageControl, State.CurrentTurnDamage);

            var sharedRotation = State.ActivePlayer == PlayerSlot.Player1 ? 180f : 0f;
            if (_turnControlsLayout is not null)
            {
                _turnControlsLayout.Rotation = sharedRotation;
            }

            if (_buyMasteryButton is not null)
            {
                var canBuyMastery = _gameService.CanBuyMastery(State);
                _buyMasteryButton.Enabled = canBuyMastery;
                _buyMasteryButton.Alpha = 1f;
            }
        }

        private void RenderPlayer(PlayerSlot slot, LinearLayout? row, TextView? identityText, View? healthControl, View? masteryControl)
        {
            var player = _gameService.GetPlayer(State, slot);
            var identity = _gameService.GetIdentity(player.IdentityName);

            if (row is not null)
            {
                row.SetBackgroundColor(State.ActivePlayer == slot ? Color.Rgb(96, 96, 96) : Color.Rgb(51, 51, 51));
            }

            if (identityText is not null)
            {
                identityText.Text = player.IdentityName ?? string.Empty;
                identityText.SetBackgroundColor(ParseColor(identity?.Color ?? "#444444"));
                identityText.SetTextColor(Color.White);
            }

            SetValueControl(healthControl, player.HealthPoints);
            SetValueControl(masteryControl, player.Mastery);
        }

        private static void SetValueIcon(View? control, int drawableResourceId)
        {
            control?.FindViewById<ImageView>(Resource.Id.controlIconImage)?.SetImageResource(drawableResourceId);
        }

        private static void SetValueTextSize(View? control, float textSizeSp)
        {
            control?.FindViewById<TextView>(Resource.Id.controlValueText)?.SetTextSize(ComplexUnitType.Sp, textSizeSp);
        }

        private static void SetValueControl(View? control, int value)
        {
            if (control is null)
            {
                return;
            }

            control.FindViewById<TextView>(Resource.Id.controlValueText)!.Text = value.ToString();
        }

        private static void UpdateIdentityButtons(LinearLayout? container, string? selectedIdentityName)
        {
            if (container is null)
            {
                return;
            }

            for (var index = 0; index < container.ChildCount; index++)
            {
                if (container.GetChildAt(index) is not Button button)
                {
                    continue;
                }

                button.Alpha = button.Text == selectedIdentityName ? 1f : 0.65f;
            }
        }

        private void ShowMessage(string message)
        {
            new AlertDialog.Builder(this)
                .SetMessage(message)
                .SetPositiveButton(Resource.String.ok, (_, _) => { })
                .Show();
        }

        private void SaveState()
        {
            _gameStateStore?.Save(State);
        }

        private GameState State => _gameState ??= _gameService.CreateInitialState();

        private static Color ParseColor(string color)
        {
            return Color.ParseColor(color);
        }
    }
}