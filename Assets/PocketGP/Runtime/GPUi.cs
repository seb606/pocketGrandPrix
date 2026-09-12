using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;

namespace PocketGP {
public sealed class GPUi : MonoBehaviour {
    GPRace game;
    Canvas canvas;
    CanvasScaler scaler;
    RectTransform layer;
    Font font;
    Text hud, count, notice, itemText;
    Image nitro;
    bool left, right;
    public bool Brake, Drift, Boost, ItemTrigger;
    public float TouchSteer { get { return (right ? 1 : 0) - (left ? 1 : 0); } }

    readonly Color ink = GPArt.Hex("F8EED9"), muted = GPArt.Hex("AEC8CC"), panel = GPArt.Hex("172E40"), mint = GPArt.Hex("54E3C5"), coral = GPArt.Hex("FF785A");

    public void Init(GPRace g) {
        game = g;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var o = new GameObject("Interface", typeof(RectTransform));
        o.transform.SetParent(transform, false);
        canvas = o.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        scaler = o.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        o.AddComponent<GraphicRaycaster>();
        if (!FindFirstObjectByType<EventSystem>()) {
            var e = new GameObject("EventSystem");
            e.AddComponent<EventSystem>();
            e.AddComponent<StandaloneInputModule>();
        }
        Responsive();
    }

    void Responsive() {
        bool portrait = Screen.height > Screen.width;
        scaler.referenceResolution = portrait ? new Vector2(760, 1200) : new Vector2(1280, 900);
        scaler.matchWidthOrHeight = portrait ? 0 : 1;
    }

    RectTransform Rect(Transform p, string n, Vector2 anchor, Vector2 pos, Vector2 size) {
        var o = new GameObject(n, typeof(RectTransform));
        o.transform.SetParent(p, false);
        var r = o.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = anchor;
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        return r;
    }

    Image Block(Transform p, Vector2 anchor, Vector2 pos, Vector2 size, Color color) {
        var r = Rect(p, "Panel", anchor, pos, size);
        var image = r.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    Text Txt(Transform p, string value, Vector2 pos, Vector2 size, int fontsize, Color color, TextAnchor align = TextAnchor.MiddleCenter) {
        var r = Rect(p, value, new Vector2(.5f, .5f), pos, size);
        var t = r.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = value;
        t.fontSize = fontsize;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    Button Button(Transform p, string title, Vector2 pos, Vector2 size, UnityAction action, bool primary = false) {
        var image = Block(p, new Vector2(.5f, .5f), pos, size, primary ? mint : GPArt.Hex("29485B"));
        var b = image.gameObject.AddComponent<Button>();
        b.targetGraphic = image;
        var colors = b.colors;
        colors.highlightedColor = new Color(1, 1, 1, .86f);
        colors.pressedColor = new Color(.72f, .82f, .86f);
        b.colors = colors;
        b.onClick.AddListener(() => {
            game.Audio.StartAudio();
            game.Audio.Beep();
            action();
        });
        Txt(image.transform, title, Vector2.zero, size - new Vector2(12, 4), 24, primary ? panel : ink);
        return b;
    }

    InputField MakeInput(Transform parent, Vector2 pos, Vector2 size, string defaultVal) {
        var box = Block(parent, new Vector2(.5f, .5f), pos, size, GPArt.Hex("223D52"));
        var input = box.gameObject.AddComponent<InputField>();
        var tObj = new GameObject("Text", typeof(RectTransform));
        tObj.transform.SetParent(box.transform, false);
        var t = tObj.AddComponent<Text>();
        t.font = font;
        t.fontSize = 24;
        t.color = ink;
        t.alignment = TextAnchor.MiddleCenter;
        var r = t.rectTransform;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
        input.textComponent = t;
        input.text = defaultVal;
        return input;
    }

    void Reset() {
        ClearInput();
        hud = count = notice = itemText = null;
        nitro = null;
        if (layer) {
            layer.gameObject.SetActive(false);
            Destroy(layer.gameObject);
        }
        layer = Rect(canvas.transform, "Ecran", new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
        layer.anchorMin = Vector2.zero;
        layer.anchorMax = Vector2.one;
        layer.offsetMin = layer.offsetMax = Vector2.zero;
    }

    RectTransform Card(string eyebrow, string title) {
        Reset();
        var veil = Block(layer, new Vector2(.5f, .5f), Vector2.zero, Vector2.zero, new Color(.025f, .08f, .12f, .6f));
        veil.rectTransform.anchorMin = Vector2.zero;
        veil.rectTransform.anchorMax = Vector2.one;
        veil.rectTransform.offsetMin = veil.rectTransform.offsetMax = Vector2.zero;
        var card = Block(layer, new Vector2(.5f, .5f), Vector2.zero, new Vector2(680, 880), new Color(panel.r, panel.g, panel.b, .98f)).rectTransform;
        Block(card, new Vector2(.5f, .5f), new Vector2(0, 434), new Vector2(680, 12), mint);
        Txt(card, eyebrow, new Vector2(0, 382), new Vector2(620, 30), 18, mint);
        Txt(card, title, new Vector2(0, 328), new Vector2(640, 80), 40, ink);
        return card;
    }

    public void ShowMenu() {
        var c = Card("PETITES VOITURES. GRANDES COURSES.", "POCKET GRAND PRIX");
        Txt(c, "Le salon devient votre terrain de jeu.", new Vector2(0, 240), new Vector2(620, 36), 22, muted);
        Txt(c, "CIRCUIT", new Vector2(0, 188), new Vector2(600, 30), 17, mint);

        // Flèches bien visibles "<" et ">" au lieu de caractères Unicode manquants
        Button(c, "<", new Vector2(-273, 140), new Vector2(54, 54), () => { game.TrackIndex = (game.TrackIndex + 2) % 3; game.BuildWorld(game.TrackIndex); ShowMenu(); });
        Txt(c, GPTrack.Names[game.TrackIndex], new Vector2(0, 144), new Vector2(480, 55), 25, ink);
        Button(c, ">", new Vector2(273, 140), new Vector2(54, 54), () => { game.TrackIndex = (game.TrackIndex + 1) % 3; game.BuildWorld(game.TrackIndex); ShowMenu(); });

        float best = PlayerPrefs.GetFloat("best_" + game.TrackIndex + "_" + game.Difficulty, 0);
        Txt(c, best > 0 ? "Record perso : " + TimeText(best) : "3 tours  ·  5 pilotes  ·  armes bonus", new Vector2(0, 96), new Vector2(620, 30), 18, muted);

        string[] dif = { "Facile", "Moyen", "Difficile" };
        for (int i = 0; i < 3; i++) {
            int k = i;
            Button(c, dif[i], new Vector2((i - 1) * 204, 38), new Vector2(194, 48), () => { game.Difficulty = k; ShowMenu(); }, game.Difficulty == i);
        }

        string[] names = { "Corail", "Menthe", "Citron", "Lilas", "Rose" };
        Button(c, "Voiture : " + names[game.ColorIndex] + "   >", new Vector2(0, -22), new Vector2(604, 48), () => { game.ColorIndex = (game.ColorIndex + 1) % 5; game.BuildWorld(game.TrackIndex); ShowMenu(); });

        Button(c, "COURSE RAPIDE", new Vector2(0, -78), new Vector2(604, 56), () => game.StartRace(), true);
        Button(c, "CHAMPIONNAT · 3 CIRCUITS", new Vector2(0, -140), new Vector2(604, 52), () => game.StartRace(true));
        Button(c, "🏆  CLASSEMENT · TOP 10", new Vector2(0, -198), new Vector2(604, 48), () => ShowLeaderboard(game.TrackIndex));

        Button(c, game.MobileMode ? "AFFICHAGE : 📱 TÉLÉPHONE (PAYSAGE)" : "AFFICHAGE : 🖥️ ORDINATEUR", new Vector2(0, -252), new Vector2(604, 46), () => {
            game.MobileMode = !game.MobileMode;
            PlayerPrefs.SetInt("display_mode", game.MobileMode ? 1 : 0);
            PlayerPrefs.Save();
            ShowMenu();
        });

        Button(c, "Comment jouer", new Vector2(-154, -304), new Vector2(296, 44), ShowHelp);
        Button(c, "Audio & affichage", new Vector2(154, -304), new Vector2(296, 44), () => ShowSettings(false));

        Button(c, "🚪  QUITTER LE JEU", new Vector2(0, -356), new Vector2(604, 44), QuitGame);
        Txt(c, game.MobileMode ? "Mode paysage recommandé · commandes tactiles à l'écran" : "Contrôles clavier ZQSD / Flèches", new Vector2(0, -398), new Vector2(620, 30), 16, muted);
    }

    public void ShowLeaderboard(int trackFilter = 0) {
        var c = Card("TABLEAU DES RECORDS", "CLASSEMENT · TOP 10");

        // Onglets pour chaque circuit
        Button(c, "Petit-déj.", new Vector2(-204, 230), new Vector2(195, 44), () => ShowLeaderboard(0), trackFilter == 0);
        Button(c, "Bureau", new Vector2(0, 230), new Vector2(195, 44), () => ShowLeaderboard(1), trackFilter == 1);
        Button(c, "Jardin", new Vector2(204, 230), new Vector2(195, 44), () => ShowLeaderboard(2), trackFilter == 2);

        var top = GPLeaderboard.GetTop(trackFilter);
        float yStart = 165f;
        for (int i = 0; i < 10; i++) {
            float y = yStart - i * 40f;
            var rowBg = Block(c, new Vector2(.5f, .5f), new Vector2(0, y), new Vector2(610, 34), i % 2 == 0 ? new Color(.1f, .22f, .3f, .6f) : new Color(.07f, .16f, .22f, .4f));
            string rankStr = i == 0 ? "🥇 1" : i == 1 ? "🥈 2" : i == 2 ? "🥉 3" : string.Format("#{0}", i + 1);

            if (i < top.Count) {
                var entry = top[i];
                Txt(rowBg.transform, rankStr, new Vector2(-260, 0), new Vector2(60, 30), 18, mint, TextAnchor.MiddleLeft);
                Txt(rowBg.transform, entry.Name, new Vector2(-130, 0), new Vector2(180, 30), 19, ink, TextAnchor.MiddleLeft);
                Txt(rowBg.transform, entry.Country, new Vector2(50, 0), new Vector2(140, 30), 18, muted, TextAnchor.MiddleLeft);
                Txt(rowBg.transform, TimeText(entry.Time), new Vector2(210, 0), new Vector2(120, 30), 19, mint, TextAnchor.MiddleRight);
            } else {
                Txt(rowBg.transform, rankStr, new Vector2(-260, 0), new Vector2(60, 30), 18, muted, TextAnchor.MiddleLeft);
                Txt(rowBg.transform, "—", new Vector2(0, 0), new Vector2(200, 30), 18, muted);
            }
        }

        Button(c, "RETOUR AU MENU", new Vector2(0, -325), new Vector2(604, 56), ShowMenu, true);
    }

    public void ShowHelp() {
        var c = Card("LE GUIDE DU PILOTE", "PETIT FORMAT, GRAND FUN");
        Txt(c, "L'accélération est automatique.\n\n<- / -> ou Q / D (A / D) : tourner\n↓ ou S : freiner · Espace : déraper\nMaj : turbo · R : retour sur la piste\nE ou Entrée : utiliser l'arme ramassée\nP ou Échap : pause\n\nTéléphone : boutons tactiles en bas de l'écran.\nMaintenir DRIFT dans les virages, puis relâcher\npour obtenir un petit turbo.\n\nLes boîtes dorées donnent une arme aléatoire !\nMissiles, flaques d'huile, super turbos et boucliers.\nLes jetons rechargent le turbo et donnent 100 points.\n\nTerminer exactement 3 tours pour l'arrivée !", new Vector2(0, -25), new Vector2(610, 525), 22, ink);
        Button(c, "RETOUR", new Vector2(0, -345), new Vector2(604, 58), ShowMenu, true);
    }

    void Slider(Transform p, string label, float y, float value, UnityAction<float> change) {
        Txt(p, label, new Vector2(0, y + 30), new Vector2(600, 34), 23, ink);
        var bg = Block(p, new Vector2(.5f, .5f), new Vector2(0, y - 10), new Vector2(550, 30), GPArt.Hex("35546A"));
        var s = bg.gameObject.AddComponent<Slider>();
        s.minValue = 0;
        s.maxValue = 1;
        var handle = Block(bg.transform, new Vector2(.5f, .5f), Vector2.zero, new Vector2(28, 42), mint);
        s.handleRect = handle.rectTransform;
        s.targetGraphic = handle;
        s.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
        s.value = value;
        s.onValueChanged.AddListener(change);
    }

    public void ShowSettings(bool paused) {
        var c = Card("À VOTRE RYTHME", "AUDIO & AFFICHAGE");
        Slider(c, "Musique", 155, game.Audio.MusicVolume, v => game.Audio.MusicVolume = v);
        Slider(c, "Effets sonores", 40, game.Audio.EffectsVolume, v => game.Audio.EffectsVolume = v);
        Button(c, "Écouter un son test", new Vector2(0, -70), new Vector2(604, 56), () => game.Audio.Beep(true));
        Button(c, "Qualité : " + (QualitySettings.shadows == ShadowQuality.Disable ? "Éco" : "Élevée"), new Vector2(0, -143), new Vector2(604, 56), () => {
            bool eco = QualitySettings.shadows != ShadowQuality.Disable;
            QualitySettings.shadows = eco ? ShadowQuality.Disable : ShadowQuality.All;
            QualitySettings.antiAliasing = eco ? 0 : 4;
            ShowSettings(paused);
        });
        Txt(c, "Rendu plein écran net haute définition.\nPlein écran : bouton en bas de la page Web.", new Vector2(0, -237), new Vector2(610, 75), 22, muted);
        Button(c, "RETOUR", new Vector2(0, -345), new Vector2(604, 58), () => {
            PlayerPrefs.Save();
            if (paused) ShowPause(); else ShowMenu();
        }, true);
    }

    void Hold(string label, Vector2 anchor, Vector2 position, Vector2 size, System.Action<bool> callback) {
        var image = Block(layer, anchor, position, size, new Color(.08f, .19f, .26f, .88f));
        Txt(image.transform, label, Vector2.zero, size, 25, ink);
        var h = image.gameObject.AddComponent<GPHold>();
        h.Changed = callback;
    }

    public void ClearInput() {
        left = right = Brake = Drift = Boost = ItemTrigger = false;
    }

    public void ShowHUD() {
        Reset();
        var header = Block(layer, new Vector2(0, 1), new Vector2(245, -64), new Vector2(460, 98), new Color(.06f, .15f, .21f, .88f));
        hud = Txt(header.transform, "", Vector2.zero, new Vector2(426, 85), 25, ink, TextAnchor.MiddleLeft);

        var pause = Button(layer, "PAUSE", Vector2.zero, new Vector2(140, 56), () => game.Pause());
        var pr = pause.GetComponent<RectTransform>();
        pr.anchorMin = pr.anchorMax = new Vector2(1, 1);
        pr.anchoredPosition = new Vector2(-88, -46);

        var bar = Block(layer, new Vector2(0, 1), new Vector2(245, -129), new Vector2(450, 12), panel);
        nitro = Block(bar.transform, new Vector2(0, .5f), Vector2.zero, new Vector2(450, 12), mint);
        nitro.rectTransform.pivot = new Vector2(0, .5f);

        // Affichage de l'arme en cours
        var itemBox = Block(layer, new Vector2(0, 1), new Vector2(580, -64), new Vector2(180, 70), new Color(.06f, .15f, .21f, .92f));
        itemText = Txt(itemBox.transform, "ARME : VIDE", Vector2.zero, new Vector2(170, 60), 19, ink);

        count = Txt(layer, "", new Vector2(0, 30), new Vector2(500, 150), 110, ink);
        notice = Txt(layer, "", new Vector2(0, 145), new Vector2(680, 55), 27, mint);

        if (game.MobileMode) {
            // Commandes tactiles optimisées pour téléphone en mode paysage
            Hold("<", new Vector2(0, 0), new Vector2(85, 95), new Vector2(125, 125), v => left = v);
            Hold(">", new Vector2(0, 0), new Vector2(225, 95), new Vector2(125, 125), v => right = v);

            Hold("ARME", new Vector2(1, 0), new Vector2(-420, 85), new Vector2(110, 105), v => { if (v) ItemTrigger = true; });
            Hold("FREIN", new Vector2(1, 0), new Vector2(-300, 85), new Vector2(110, 105), v => Brake = v);
            Hold("DRIFT", new Vector2(1, 0), new Vector2(-180, 85), new Vector2(115, 105), v => Drift = v);
            Hold("TURBO", new Vector2(1, 0), new Vector2(-60, 105), new Vector2(115, 145), v => Boost = v);
        }
    }

    public void QuitGame() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
        ShowQuitScreen();
    }

    public void ShowQuitScreen() {
        var c = Card("AU REVOIR", "POCKET GRAND PRIX");
        Txt(c, "Merci d'avoir joué à Pocket Grand Prix !\n\nVous pouvez fermer cet onglet ou retourner au jeu.", new Vector2(0, 40), new Vector2(600, 140), 24, ink);
        Button(c, "RETOURNER AU MENU", new Vector2(0, -120), new Vector2(604, 60), ShowMenu, true);
    }

    public void ShowPause() {
        var c = Card("ON SOUFFLE UN PEU", "PAUSE AU STAND");
        Button(c, "REPRENDRE", new Vector2(0, 110), new Vector2(604, 70), () => game.Pause(), true);
        Button(c, "Audio & affichage", new Vector2(0, 15), new Vector2(604, 60), () => ShowSettings(true));
        Button(c, "Recommencer", new Vector2(0, -75), new Vector2(604, 60), () => game.Retry());
        Button(c, "Menu principal", new Vector2(0, -165), new Vector2(604, 60), () => game.Menu());
    }

    public void ShowResults() {
        var c = Card(GPTrack.Names[game.TrackIndex].ToUpperInvariant(), game.ResultPlace == 1 ? "VICTOIRE !" : "COURSE TERMINÉE");
        Txt(c, game.ResultPlace + " / 5", new Vector2(0, 195), new Vector2(600, 90), 70, mint);
        Txt(c, "Temps : " + TimeText(game.FinalTime) + "   ·   Score : " + game.Score + " pts", new Vector2(0, 130), new Vector2(600, 40), 25, ink);

        // Section enregistrement Top 10
        Txt(c, "ENREGISTRER VOTRE SCORE AU TOP 10", new Vector2(0, 75), new Vector2(600, 30), 18, mint);

        Txt(c, "Pseudo :", new Vector2(-220, 25), new Vector2(120, 34), 19, muted, TextAnchor.MiddleRight);
        string lastPseudo = PlayerPrefs.GetString("last_pseudo", "Pilote");
        var input = MakeInput(c, new Vector2(-40, 25), new Vector2(200, 44), lastPseudo);

        Txt(c, "Pays :", new Vector2(100, 25), new Vector2(80, 34), 19, muted, TextAnchor.MiddleRight);
        int cIndex = PlayerPrefs.GetInt("last_country", 0);
        Button countryBtn = null;
        countryBtn = Button(c, GPLeaderboard.Countries[cIndex], new Vector2(215, 25), new Vector2(160, 44), () => {
            cIndex = (cIndex + 1) % GPLeaderboard.Countries.Length;
            var t = countryBtn.GetComponentInChildren<Text>();
            if (t) t.text = GPLeaderboard.Countries[cIndex];
        });

        Button(c, "💾  ENREGISTRER MON SCORE", new Vector2(0, -40), new Vector2(604, 52), () => {
            string pseudo = input.text.Trim();
            if (string.IsNullOrEmpty(pseudo)) pseudo = "Pilote";
            PlayerPrefs.SetString("last_pseudo", pseudo);
            PlayerPrefs.SetInt("last_country", cIndex);
            PlayerPrefs.Save();
            GPLeaderboard.AddEntry(pseudo, GPLeaderboard.Countries[cIndex], game.FinalTime, game.TrackIndex, game.Score);
            ShowLeaderboard(game.TrackIndex);
        }, true);

        if (game.Championship) Button(c, game.TrackIndex < 2 ? "CIRCUIT SUIVANT" : "RÉSULTAT DU CHAMPIONNAT", new Vector2(0, -115), new Vector2(604, 52), () => game.Next());
        else Button(c, "REJOUER", new Vector2(0, -115), new Vector2(604, 52), () => game.Retry());

        Button(c, "🏆  VOIR LE CLASSEMENT", new Vector2(0, -180), new Vector2(604, 48), () => ShowLeaderboard(game.TrackIndex));
        Button(c, "MENU PRINCIPAL", new Vector2(0, -242), new Vector2(604, 48), () => game.Menu());
    }

    public void ShowChampionship() {
        var c = Card("LES TROIS CIRCUITS SONT TERMINÉS", "FIN DU CHAMPIONNAT");
        Txt(c, game.ChampionshipPoints + " / 30", new Vector2(0, 115), new Vector2(620, 110), 70, mint);
        Txt(c, game.ChampionshipPoints >= 25 ? "Trophée OR" : game.ChampionshipPoints >= 18 ? "Trophée ARGENT" : "Trophée BRONZE", new Vector2(0, 8), new Vector2(600, 60), 34, ink);
        Button(c, "MENU PRINCIPAL", new Vector2(0, -180), new Vector2(604, 66), () => game.Menu(), true);
    }

    public static string TimeText(float t) {
        return ((int)t / 60).ToString("00") + ":" + ((int)t % 60).ToString("00") + "." + ((int)(t * 100) % 100).ToString("00");
    }

    public void Refresh() {
        Responsive();
        if (hud && game.Cars.Count > 0) {
            hud.text = game.Position() + " / 5     TOUR " + Mathf.Min(game.Cars[0].Laps + 1, GPRace.TotalLaps) + " / " + GPRace.TotalLaps + "\n" + TimeText(game.RaceTime) + "     " + game.Score + " pts";
            nitro.rectTransform.sizeDelta = new Vector2(450 * game.Cars[0].Nitro, 12);
            count.text = game.State == GPState.Countdown ? Mathf.Max(1, Mathf.CeilToInt(game.Countdown)).ToString() : "";
            notice.text = game.Notice;

            if (itemText) {
                string[] itemLabels = { "ARME : VIDE", "MISSILE (E)", "HUILE (E)", "TURBO (E)", "BOUCLIER (E)" };
                int idx = (int)game.Cars[0].CurrentItem;
                itemText.text = itemLabels[idx];
                itemText.color = idx == 0 ? muted : mint;
            }
        }
    }
}
}
