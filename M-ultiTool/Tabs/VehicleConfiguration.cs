using MultiTool.Core;
using MultiTool.Extensions;
using MultiTool.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiTool.Modules;
using Logger = MultiTool.Modules.Logger;
using System.Threading;
using static mainscript;

namespace MultiTool.Tabs
{
	internal class VehicleConfigurationTab : Tab
	{
		public override string Name => "Vehicle Configuration";

		private Vector2 currentVehiclePosition;
        private Vector2 currentTabPosition;
        private float lastHeight = 500f;
        private int lastCarId = 0;
        private string tab = null;
        private readonly string[] tabs = new string[]
        {
            "Basics",
            "Fluids",
            "Glass",
            "Material changer",
            "Randomised changer",
            "Light changer",
        };

		private Settings settings = new Settings();

		private Dictionary<string, string> materials = new Dictionary<string, string>()
		{
			{ "coloredleather", "Seat Leather" },
			{ "leather", "Sun visor leather" },
            { "chleathercar08c", "Leather" },
            { "fociw", "Leather 2" },
            { "focib", "Leather 3" },
            { "cleather", "Leather 4" },
			{ "huzat01", "Fabric 1" },
			{ "huzat02", "Fabric 2" },
			{ "huzat03", "Fabric 3" },
			{ "huzat04", "Fabric 4" },
			{ "karpit", "Cardboard" },
			{ "wood", "Wood" },
			{ "firearmwood", "Wood 2" },
            { "busfa", "Wood 3" },
			{ "metals", "Metal" },
			{ "metals2", "Metal 2" },
            { "buslepcso", "Metal 3" },
            { "buspadlo", "Metal 4" },
			{ "darkmetal", "Dark metal" },
			{ "regilampaszin", "Lamp metal" },
			{ "gumi", "Tire rubber" },
            { "fehergumi", "Tire rubber 2" },
			{ "nyulsz01", "Rabbit fur" },
			{ "szivacs2", "Sponge" },
			{ "tarbanckarpit", "Bakelite" },
            { "csodapaint", "Metal painted" },
            { "car08paint", "Metal painted 2" },
            { "tarbancpaint", "Duroplast painted" },
            { "busfem", "Plastic 1" },
            { "busteto", "Plastic 2" },
            { "metalrevolver", "Revolver" },
            { "radiator", "Radiator" },
            { "car07csik", "Fury stripe white" },
            { "car07csik2", "Fury stripe gold" },
        };
		private bool partSelectorOpen = false;
		private bool materialSelectorOpen = false;
		private PartGroup selectedPart = null;
		private string selectedMaterial = null;
		private bool colorSelectorOpen = false;

		// Random selector.
		private bool randomSelectorOpen = false;
		private randomTypeSelector selectedRandom = null;

        // Light changer.
        private bool lightSelectorOpen = false;
        private List<LightGroup> selectedLights = new List<LightGroup>();

		// Caching.
		private List<PartGroup> materialParts = new List<PartGroup>();
		private List<randomTypeSelector> randomParts = new List<randomTypeSelector>();
        private List<LightGroup> lights = new List<LightGroup>();
		private float nextUpdateTime = 0;
		private float updateFrequency = 2;

		public override void RenderTab(Dimensions dimensions)
		{
            if (tab == null)
                tab = tabs[0];

			bool refreshedCache = false;
			nextUpdateTime -= Time.fixedDeltaTime;

			int columns = 2;
			if (dimensions.width <= 650f)
				columns = 1;

            float tabX = dimensions.x + 10f;
            float tabY = dimensions.y + 10f;
            float tabWidth = (dimensions.width - 20f) * 0.1f;
            float startingCurrVehicleX = dimensions.x + tabWidth + 20f;
			float currVehicleX = startingCurrVehicleX;
			float currVehicleY = dimensions.y + 10f;
			float buttonWidth = 200f;
			float buttonHeight = 20f;
			float sliderWidth = 300f;
			float headerWidth = (dimensions.width / 2) - 20f;
			float headerHeight = 40f;

			if (columns == 1)
				headerWidth = dimensions.width - 20f;

			if (mainscript.M.player.Car == null)
			{
				GUI.Label(new Rect(currVehicleX, currVehicleY, dimensions.width - 20f, dimensions.height - 20f), "No current vehicle\nSit in a vehicle to show configuration", GUIRenderer.messageStyle);
				nextUpdateTime = 0;
				return;
			}

			carscript car = mainscript.M.player.Car;
			GameObject carObject = car.gameObject;
			partconditionscript partconditionscript = car.gameObject.GetComponent<partconditionscript>();
			coolantTankscript coolant = car.coolant;
			enginescript engine = car.Engine;
			tankscript fuel = car.Tank;
			Transform sunRoofSlot = carObject.transform.FindRecursive("SunRoofSlot");
			tosaveitemscript save = carObject.GetComponent<tosaveitemscript>();

            // Reset any selections when changing car.
            if (save.idInSave != lastCarId)
            {
                selectedPart = null;
                selectedRandom = null;
                selectedLights.Clear();
            }

            GUI.Box(new Rect(tabX, tabY, tabWidth, dimensions.height - 20f), string.Empty);
            tabY += 10f;

            currentTabPosition = GUI.BeginScrollView(new Rect(tabX, tabY, tabWidth, dimensions.height - 20f), currentTabPosition, new Rect(tabX, tabY, (dimensions.width - 20f) * 0.1f, (tabs.Length * 25f) + 10f));

            foreach (string tabName in tabs)
            {
                if (GUI.Button(new Rect(tabX + 10f, tabY, tabWidth - 20f, 20f), GUIRenderer.GetAccessibleString(tabName, tab == tabName)))
                    tab = tabName;
                tabY += 25f;
            }

            GUI.EndScrollView();

			currentVehiclePosition = GUI.BeginScrollView(new Rect(currVehicleX, currVehicleY, dimensions.width - (dimensions.width * 0.1f) - 20f, dimensions.height - 20f), currentVehiclePosition, new Rect(currVehicleX, currVehicleY, dimensions.width - 20f, lastHeight - 20f), new GUIStyle(), GUI.skin.verticalScrollbar);

            GUIStyle defaultStyle = GUI.skin.button;
            GUIStyle previewStyle = new GUIStyle(defaultStyle);
            Texture2D previewTexture = new Texture2D(1, 1);
            Color[] pixels = null;

            switch (tab)
            {
                case "Basics":
                    // Vehicle god mode.
                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Vehicle god mode", car.crashMultiplier <= 0.0)))
                    {
                        car.crashMultiplier *= -1f;
                    }

                    currVehicleY += buttonHeight + 10f;

                    // Toggle slot mover.
                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleString("Toggle slot mover", settings.mode == "slotControl")))
                    {
                        if (settings.mode == "slotControl")
                        {
                            GUIRenderer.SlotMoverDispose();
                        }
                        else
                        {
                            settings.mode = "slotControl";
                            settings.car = car;
                            settings.slotStage = "slotSelect";
                        }
                    }

                    currVehicleY += buttonHeight + 10f;

                    // Condition.
                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Condition", GUIRenderer.headerStyle);
                    currVehicleY += headerHeight;
                    int maxCondition = (int)Enum.GetValues(typeof(Item.Condition)).Cast<Item.Condition>().Max();
                    float rawCondition = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.conditionInt, 0, maxCondition);
                    GUIRenderer.conditionInt = Mathf.RoundToInt(rawCondition);
                    currVehicleX += sliderWidth + 10f;

                    GUIRenderer.applyConditionToAttached = GUI.Toggle(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.applyConditionToAttached, "Apply to attached");

                    currVehicleX += buttonWidth * 0.8f;

                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
                    {
                        GameUtilities.SetCondition(GUIRenderer.conditionInt, GUIRenderer.applyConditionToAttached, partconditionscript);
                    }
                    currVehicleX = startingCurrVehicleX;
                    currVehicleY += buttonHeight;
                    string conditionName = ((Item.Condition)GUIRenderer.conditionInt).ToString();
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), conditionName, GUIRenderer.labelStyle);

                    currVehicleY += buttonHeight + 10f;

                    // Vehicle colour sliders.
                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Color", GUIRenderer.headerStyle);
                    currVehicleY += headerHeight;
                    // Red.
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Red:", new Color(255, 0, 0)), GUIRenderer.labelStyle);
                    currVehicleY += buttonHeight;
                    float red = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.color.r * 255, 0, 255);
                    red = Mathf.Round(red);
                    currVehicleY += buttonHeight;
                    bool redParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), red.ToString(), GUIRenderer.labelStyle), out red);
                    if (!redParse)
                        Logger.Log($"{redParse} is not a number", Logger.LogLevel.Error);
                    red = Mathf.Clamp(red, 0f, 255f);
                    GUIRenderer.color.r = red / 255f;

                    // Green.
                    currVehicleY += buttonHeight + 10f;
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Green:", new Color(0, 255, 0)), GUIRenderer.labelStyle);
                    currVehicleY += buttonHeight;
                    float green = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.color.g * 255, 0, 255);
                    green = Mathf.Round(green);
                    currVehicleY += buttonHeight;
                    bool greenParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), green.ToString(), GUIRenderer.labelStyle), out green);
                    if (!greenParse)
                        Logger.Log($"{greenParse} is not a number", Logger.LogLevel.Error);
                    green = Mathf.Clamp(green, 0f, 255f);
                    GUIRenderer.color.g = green / 255f;

                    // Blue.
                    currVehicleY += buttonHeight + 10f;
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Blue:", new Color(0, 0, 255)), GUIRenderer.labelStyle);
                    currVehicleY += buttonHeight;
                    float blue = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.color.b * 255, 0, 255);
                    blue = Mathf.Round(blue);
                    currVehicleY += buttonHeight;
                    bool blueParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), blue.ToString(), GUIRenderer.labelStyle), out blue);
                    if (!blueParse)
                        Logger.Log($"{blueParse.ToString()} is not a number", Logger.LogLevel.Error);
                    blue = Mathf.Clamp(blue, 0f, 255f);
                    GUIRenderer.color.b = blue / 255f;

                    currVehicleY += buttonHeight + 10f;

                    // Colour preview.
                    pixels = new Color[] { GUIRenderer.color };
                    previewTexture.SetPixels(pixels);
                    previewTexture.Apply();
                    previewStyle.normal.background = previewTexture;
                    previewStyle.active.background = previewTexture;
                    previewStyle.hover.background = previewTexture;
                    previewStyle.margin = new RectOffset(0, 0, 0, 0);
                    GUI.skin.button = previewStyle;
                    GUI.Button(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), "");
                    GUI.skin.button = defaultStyle;

                    currVehicleY += buttonHeight + 10f;
                    GUIRenderer.color = GUIRenderer.RenderColourPalette(currVehicleX, currVehicleY, sliderWidth + 20f, GUIRenderer.color);
                    currVehicleY += GUIRenderer.GetPaletteHeight(sliderWidth + 20f) + 10f;

                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Randomise colour"))
                    {
                        GUIRenderer.color.r = UnityEngine.Random.Range(0f, 255f) / 255f;
                        GUIRenderer.color.g = UnityEngine.Random.Range(0f, 255f) / 255f;
                        GUIRenderer.color.b = UnityEngine.Random.Range(0f, 255f) / 255f;
                    }

                    currVehicleX += buttonWidth + 10f;

                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
                    {
                        GameUtilities.Paint(GUIRenderer.color, partconditionscript);
                    }

                    lastHeight = currVehicleY;
                    break;

                case "Fluids":
                    // Coolant settings.
                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Radiator settings", GUIRenderer.headerStyle);
                    currVehicleY += headerHeight;
                    if (coolant != null)
                    {
                        float coolantMax = coolant.coolant.F.maxC;
                        int coolantPercentage = 0;

                        foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.coolants)
                        {
                            coolantPercentage += fluid.Value;
                        }

                        if (coolantPercentage > 100)
                            coolantPercentage = 100;

                        bool changed = false;

                        // Deep copy coolants dictionary.
                        Dictionary<mainscript.fluidenum, int> tempCoolants = GUIRenderer.coolants.ToDictionary(fluid => fluid.Key, fluid => fluid.Value);

                        foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.coolants)
                        {
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth / 3, buttonHeight), fluid.Key.ToString().ToSentenceCase(), GUIRenderer.labelStyle);
                            currVehicleX += buttonWidth / 3;
                            int percentage = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), fluid.Value, 0, 100));
                            if (percentage + (coolantPercentage - fluid.Value) <= 100)
                            {
                                tempCoolants[fluid.Key] = percentage;
                                changed = true;
                            }
                            currVehicleX += sliderWidth + 5f;
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), $"{percentage}%", GUIRenderer.labelStyle);
                            currVehicleX = startingCurrVehicleX;
                            currVehicleY += buttonHeight;
                        }

                        if (changed)
                            GUIRenderer.coolants = tempCoolants;

                        if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Get current"))
                        {
                            tankscript tank = coolant.coolant;

                            tempCoolants = new Dictionary<fluidenum, int>();
                            foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.coolants)
                            {
                                tempCoolants[fluid.Key] = 0;
                            }

                            GUIRenderer.coolants = tempCoolants;

                            foreach (fluid fluid in tank.F.fluids)
                            {
                                int percentage = (int)(fluid.amount / tank.F.maxC * 100);
                                GUIRenderer.coolants[fluid.type] = percentage;
                            }
                        }

                        currVehicleX += buttonWidth + 10f;

                        if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
                        {
                            tankscript tank = coolant.coolant;
                            tank.F.fluids.Clear();
                            foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.coolants)
                            {
                                if (fluid.Value > 0)
                                {
                                    tank.F.ChangeOne((coolantMax / 100) * fluid.Value, fluid.Key);
                                }
                            }
                        }

                        currVehicleX = startingCurrVehicleX;
                    }
                    else
                        GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "No radiator found.", GUIRenderer.labelStyle);

                    currVehicleY += buttonHeight + 20f;

                    // Engine settings.
                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Engine settings", GUIRenderer.headerStyle);
                    currVehicleY += headerHeight;
                    if (engine != null)
                    {
                        if (engine.T != null)
                        {
                            float oilMax = engine.T.F.maxC;
                            int oilPercentage = 0;

                            foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.oils)
                            {
                                oilPercentage += fluid.Value;
                            }

                            if (oilPercentage > 100)
                                oilPercentage = 100;

                            bool changed = false;

                            // Deep copy oils dictionary.
                            Dictionary<mainscript.fluidenum, int> tempOils = GUIRenderer.oils.ToDictionary(fluid => fluid.Key, fluid => fluid.Value);

                            foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.oils)
                            {
                                GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth / 3, buttonHeight), fluid.Key.ToString().ToSentenceCase(), GUIRenderer.labelStyle);
                                currVehicleX += buttonWidth / 3;
                                int percentage = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), fluid.Value, 0, 100));
                                if (percentage + (oilPercentage - fluid.Value) <= 100)
                                {
                                    tempOils[fluid.Key] = percentage;
                                    changed = true;
                                }
                                currVehicleX += sliderWidth + 5f;
                                GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), $"{percentage}%", GUIRenderer.labelStyle);
                                currVehicleX = startingCurrVehicleX;
                                currVehicleY += buttonHeight;
                            }

                            if (changed)
                                GUIRenderer.oils = tempOils;

                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Get current"))
                            {
                                tankscript tank = engine.T;

                                tempOils = new Dictionary<fluidenum, int>();
                                foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.oils)
                                {
                                    tempOils[fluid.Key] = 0;
                                }

                                GUIRenderer.oils = tempOils;

                                foreach (fluid fluid in tank.F.fluids)
                                {
                                    int percentage = (int)(fluid.amount / tank.F.maxC * 100);
                                    GUIRenderer.oils[fluid.type] = percentage;
                                }
                            }

                            currVehicleX += buttonWidth + 10f;

                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
                            {
                                tankscript tank = engine.T;
                                tank.F.fluids.Clear();
                                foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.oils)
                                {
                                    if (fluid.Value > 0)
                                    {
                                        tank.F.ChangeOne((oilMax / 100) * fluid.Value, fluid.Key);
                                    }
                                }
                            }

                            currVehicleX = startingCurrVehicleX;
                        }
                        else
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Engine has no oil tank.", GUIRenderer.labelStyle);
                    }
                    else
                        GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "No engine found.", GUIRenderer.labelStyle);

                    currVehicleY += buttonHeight + 20f;

                    // Fuel settings.
                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Fuel settings", GUIRenderer.headerStyle);
                    currVehicleY += headerHeight;
                    if (fuel != null)
                    {
                        float fuelMax = fuel.F.maxC;
                        int fuelPercentage = 0;

                        foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.fuels)
                        {
                            fuelPercentage += fluid.Value;
                        }

                        if (fuelPercentage > 100)
                            fuelPercentage = 100;

                        bool changed = false;

                        // Deep copy fuels dictionary.
                        Dictionary<mainscript.fluidenum, int> tempFuels = GUIRenderer.fuels.ToDictionary(fluid => fluid.Key, fluid => fluid.Value);

                        foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.fuels)
                        {
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth / 3, buttonHeight), fluid.Key.ToString().ToSentenceCase(), GUIRenderer.labelStyle);
                            currVehicleX += buttonWidth / 3;
                            int percentage = Mathf.RoundToInt(GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), fluid.Value, 0, 100));
                            if (percentage + (fuelPercentage - fluid.Value) <= 100)
                            {
                                tempFuels[fluid.Key] = percentage;
                                changed = true;
                            }
                            currVehicleX += sliderWidth + 5f;
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), $"{percentage}%", GUIRenderer.labelStyle);
                            currVehicleX = startingCurrVehicleX;
                            currVehicleY += buttonHeight;
                        }

                        if (changed)
                            GUIRenderer.fuels = tempFuels;

                        if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Get current"))
                        {
                            tankscript tank = fuel;

                            tempFuels = new Dictionary<fluidenum, int>();
                            foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.fuels)
                            {
                                tempFuels[fluid.Key] = 0;
                            }

                            GUIRenderer.fuels = tempFuels;

                            foreach (fluid fluid in tank.F.fluids)
                            {
                                int percentage = (int)(fluid.amount / tank.F.maxC * 100);
                                GUIRenderer.fuels[fluid.type] = percentage;
                            }
                        }

                        currVehicleX += buttonWidth + 10f;

                        if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
                        {
                            tankscript tank = fuel;
                            tank.F.fluids.Clear();
                            foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.fuels)
                            {
                                if (fluid.Value > 0)
                                {
                                    tank.F.ChangeOne((fuelMax / 100) * fluid.Value, fluid.Key);
                                }
                            }
                        }

                        if (engine != null)
                        {
                            currVehicleX += buttonWidth + 10f;

                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Fill with correct fuel"))
                            {
                                fuel.F.fluids.Clear();

                                // Find the correct fluid(s) from the engine.
                                List<mainscript.fluidenum> fluids = new List<mainscript.fluidenum>();
                                foreach (fluid fluid in engine.FuelConsumption.fluids)
                                {
                                    fluids.Add(fluid.type);
                                }

                                if (fluids.Count > 0)
                                {
                                    // Two stoke.
                                    if (fluids.Contains(mainscript.fluidenum.oil) && fluids.Contains(mainscript.fluidenum.gas))
                                    {
                                        fuel.F.ChangeOne(fuelMax / 100 * 97, mainscript.fluidenum.gas);
                                        fuel.F.ChangeOne(fuelMax / 100 * 3, mainscript.fluidenum.oil);
                                    }
                                    else
                                    {
                                        // Just use the first fluid found by default.
                                        // Only mixed fuel currently is two-stroke which we're
                                        // accounting for already.
                                        fuel.F.ChangeOne(fuelMax, fluids[0]);
                                    }
                                }

                                // Update UI.
                                tempFuels = new Dictionary<fluidenum, int>();
                                foreach (KeyValuePair<mainscript.fluidenum, int> fluid in GUIRenderer.fuels)
                                {
                                    tempFuels[fluid.Key] = 0;
                                }

                                GUIRenderer.fuels = tempFuels;

                                foreach (fluid fluid in fuel.F.fluids)
                                {
                                    int percentage = (int)(fluid.amount / fuel.F.maxC * 100);
                                    GUIRenderer.fuels[fluid.type] = percentage;
                                }
                            }
                        }

                        currVehicleX = startingCurrVehicleX;
                    }
                    else
                        GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "No fuel tank found.", GUIRenderer.labelStyle);
                    lastHeight = currVehicleY;
                    break;

                case "Glass":
                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Window settings", GUIRenderer.headerStyle);
                    currVehicleY += headerHeight;

                    // Window colour sliders.
                    // Red.
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Red:", new Color(255, 0, 0)), GUIRenderer.labelStyle);
                    currVehicleY += buttonHeight;
                    float windowRed = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.windowColor.r * 255, 0, 255);
                    windowRed = Mathf.Round(windowRed);
                    currVehicleY += buttonHeight;
                    bool windowRedParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), windowRed.ToString(), GUIRenderer.labelStyle), out windowRed);
                    if (!windowRedParse)
                        Logger.Log($"{windowRedParse} is not a number", Logger.LogLevel.Error);
                    windowRed = Mathf.Clamp(windowRed, 0f, 255f);
                    GUIRenderer.windowColor.r = windowRed / 255f;

                    // Green.
                    currVehicleY += buttonHeight + 10f;
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Green:", new Color(0, 255, 0)), GUIRenderer.labelStyle);
                    currVehicleY += buttonHeight;
                    float windowGreen = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.windowColor.g * 255, 0, 255);
                    windowGreen = Mathf.Round(windowGreen);
                    currVehicleY += buttonHeight;
                    bool windowGreenParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), windowGreen.ToString(), GUIRenderer.labelStyle), out windowGreen);
                    if (!windowGreenParse)
                        Logger.Log($"{windowGreenParse} is not a number", Logger.LogLevel.Error);
                    windowGreen = Mathf.Clamp(windowGreen, 0f, 255f);
                    GUIRenderer.windowColor.g = windowGreen / 255f;

                    // Blue.
                    currVehicleY += buttonHeight + 10f;
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Blue:", new Color(0, 0, 255)), GUIRenderer.labelStyle);
                    currVehicleY += buttonHeight;
                    float windowBlue = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.windowColor.b * 255, 0, 255);
                    windowBlue = Mathf.Round(windowBlue);
                    currVehicleY += buttonHeight;
                    bool windowBlueParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), windowBlue.ToString(), GUIRenderer.labelStyle), out windowBlue);
                    if (!windowBlueParse)
                        Logger.Log($"{windowBlueParse.ToString()} is not a number", Logger.LogLevel.Error);
                    windowBlue = Mathf.Clamp(windowBlue, 0f, 255f);
                    GUIRenderer.windowColor.b = windowBlue / 255f;

                    // Alpha.
                    currVehicleY += buttonHeight + 10f;
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Alpha:", GUIRenderer.labelStyle);
                    currVehicleY += buttonHeight;
                    float windowAlpha = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.windowColor.a * 255, 0, 255);
                    windowAlpha = Mathf.Round(windowAlpha);
                    currVehicleY += buttonHeight;
                    bool windowAlphaParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), windowAlpha.ToString(), GUIRenderer.labelStyle), out windowAlpha);
                    if (!windowAlphaParse)
                        Logger.Log($"{windowAlphaParse.ToString()} is not a number", Logger.LogLevel.Error);
                    windowAlpha = Mathf.Clamp(windowAlpha, 0f, 255f);
                    GUIRenderer.windowColor.a = windowAlpha / 255f;

                    currVehicleY += buttonHeight + 10f;

                    // Colour preview.
                    // Override alpha for colour preview.
                    Color windowPreview = GUIRenderer.windowColor;
                    windowPreview.a = 1;
                    pixels = new Color[] { windowPreview };
                    previewTexture.SetPixels(pixels);
                    previewTexture.Apply();
                    previewStyle.normal.background = previewTexture;
                    previewStyle.active.background = previewTexture;
                    previewStyle.hover.background = previewTexture;
                    previewStyle.margin = new RectOffset(0, 0, 0, 0);
                    GUI.skin.button = previewStyle;
                    GUI.Button(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), "");
                    GUI.skin.button = defaultStyle;

                    currVehicleY += buttonHeight + 10f;

                    GUIRenderer.windowColor = GUIRenderer.RenderColourPalette(currVehicleX, currVehicleY, sliderWidth + 20f, GUIRenderer.windowColor);
                    currVehicleY += GUIRenderer.GetPaletteHeight(sliderWidth + 20f) + 10f;

                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Randomise colour"))
                    {
                        GUIRenderer.windowColor.r = UnityEngine.Random.Range(0f, 255f) / 255f;
                        GUIRenderer.windowColor.g = UnityEngine.Random.Range(0f, 255f) / 255f;
                        GUIRenderer.windowColor.b = UnityEngine.Random.Range(0f, 255f) / 255f;
                    }

                    currVehicleX += buttonWidth + 10f;

                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
                    {
                        List<MeshRenderer> renderers = carObject.GetComponentsInChildren<MeshRenderer>().ToList();
                        foreach (MeshRenderer meshRenderer in renderers)
                        {
                            string materialName = meshRenderer.material.name.Replace(" (Instance)", "");
                            switch (materialName)
                            {
                                // Outer glass.
                                case "Glass":
                                    // Use selected colour.
                                    meshRenderer.material.color = GUIRenderer.windowColor;
                                    break;

                                // Inner glass.
                                case "GlassNoReflection":
                                    // Use a more transparent version of the selected colour
                                    // for the inner glass to ensure it's still see-through.
                                    Color innerColor = GUIRenderer.windowColor;
                                    if (innerColor.a > 0.2f)
                                        innerColor.a = 0.2f;
                                    meshRenderer.material.color = innerColor;
                                    break;
                            }
                        }

                        SaveUtilities.UpdateGlass(new GlassData() { ID = save.idInSave, color = GUIRenderer.windowColor, type = "windows" });
                    }

                    currVehicleX = startingCurrVehicleX;
                    currVehicleY += buttonHeight + 10f;

                    // Sunroof settings.
                    if (sunRoofSlot != null)
                    {
                        currVehicleY += buttonHeight + 20f;
                        GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Sunroof settings", GUIRenderer.headerStyle);
                        currVehicleY += headerHeight;

                        Transform outerGlass = sunRoofSlot.FindRecursive("sunroof outer glass", exact: false);
                        if (outerGlass != null)
                        {
                            MeshRenderer meshRenderer = outerGlass.GetComponent<MeshRenderer>();

                            // Sunroof colour sliders.
                            // Red.
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Red:", new Color(255, 0, 0)), GUIRenderer.labelStyle);
                            currVehicleY += buttonHeight;
                            float sunroofRed = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.sunRoofColor.r * 255, 0, 255);
                            sunroofRed = Mathf.Round(sunroofRed);
                            currVehicleY += buttonHeight;
                            bool sunroofRedParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), sunroofRed.ToString(), GUIRenderer.labelStyle), out sunroofRed);
                            if (!sunroofRedParse)
                                Logger.Log($"{sunroofRedParse} is not a number", Logger.LogLevel.Error);
                            sunroofRed = Mathf.Clamp(sunroofRed, 0f, 255f);
                            GUIRenderer.sunRoofColor.r = sunroofRed / 255f;

                            // Green.
                            currVehicleY += buttonHeight + 10f;
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Green:", new Color(0, 255, 0)), GUIRenderer.labelStyle);
                            currVehicleY += buttonHeight;
                            float sunroofGreen = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.sunRoofColor.g * 255, 0, 255);
                            sunroofGreen = Mathf.Round(sunroofGreen);
                            currVehicleY += buttonHeight;
                            bool sunroofGreenParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), sunroofGreen.ToString(), GUIRenderer.labelStyle), out sunroofGreen);
                            if (!sunroofGreenParse)
                                Logger.Log($"{sunroofGreenParse} is not a number", Logger.LogLevel.Error);
                            sunroofGreen = Mathf.Clamp(sunroofGreen, 0f, 255f);
                            GUIRenderer.sunRoofColor.g = sunroofGreen / 255f;

                            // Blue.
                            currVehicleY += buttonHeight + 10f;
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Blue:", new Color(0, 0, 255)), GUIRenderer.labelStyle);
                            currVehicleY += buttonHeight;
                            float sunroofBlue = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.sunRoofColor.b * 255, 0, 255);
                            sunroofBlue = Mathf.Round(sunroofBlue);
                            currVehicleY += buttonHeight;
                            bool sunroofBlueParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), sunroofBlue.ToString(), GUIRenderer.labelStyle), out sunroofBlue);
                            if (!sunroofBlueParse)
                                Logger.Log($"{sunroofBlueParse.ToString()} is not a number", Logger.LogLevel.Error);
                            sunroofBlue = Mathf.Clamp(sunroofBlue, 0f, 255f);
                            GUIRenderer.sunRoofColor.b = sunroofBlue / 255f;

                            // Alpha.
                            currVehicleY += buttonHeight + 10f;
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Alpha:", GUIRenderer.labelStyle);
                            currVehicleY += buttonHeight;
                            float sunroofAlpha = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.sunRoofColor.a * 255, 0, 255);
                            sunroofAlpha = Mathf.Round(sunroofAlpha);
                            currVehicleY += buttonHeight;
                            bool sunroofAlphaParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), sunroofAlpha.ToString(), GUIRenderer.labelStyle), out sunroofAlpha);
                            if (!sunroofAlphaParse)
                                Logger.Log($"{sunroofAlphaParse.ToString()} is not a number", Logger.LogLevel.Error);
                            sunroofAlpha = Mathf.Clamp(sunroofAlpha, 0f, 255f);
                            GUIRenderer.sunRoofColor.a = sunroofAlpha / 255f;

                            currVehicleY += buttonHeight + 10f;

                            // Colour preview.
                            // Override alpha for colour preview.
                            Color sunroofPreview = GUIRenderer.sunRoofColor;
                            sunroofPreview.a = 1;
                            pixels = new Color[] { sunroofPreview };
                            previewTexture.SetPixels(pixels);
                            previewTexture.Apply();
                            previewStyle.normal.background = previewTexture;
                            previewStyle.active.background = previewTexture;
                            previewStyle.hover.background = previewTexture;
                            previewStyle.margin = new RectOffset(0, 0, 0, 0);
                            GUI.skin.button = previewStyle;
                            GUI.Button(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), "");
                            GUI.skin.button = defaultStyle;

                            currVehicleY += buttonHeight + 10f;
                            GUIRenderer.sunRoofColor = GUIRenderer.RenderColourPalette(currVehicleX, currVehicleY, sliderWidth + 20f, GUIRenderer.sunRoofColor);
                            currVehicleY += GUIRenderer.GetPaletteHeight(sliderWidth + 20f) + 10f;

                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Randomise colour"))
                            {
                                GUIRenderer.sunRoofColor.r = UnityEngine.Random.Range(0f, 255f) / 255f;
                                GUIRenderer.sunRoofColor.g = UnityEngine.Random.Range(0f, 255f) / 255f;
                                GUIRenderer.sunRoofColor.b = UnityEngine.Random.Range(0f, 255f) / 255f;
                            }

                            currVehicleX += buttonWidth + 10f;

                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
                            {
                                meshRenderer.material.color = GUIRenderer.sunRoofColor;

                                SaveUtilities.UpdateGlass(new GlassData() { ID = save.idInSave, color = GUIRenderer.sunRoofColor, type = "sunroof" });
                            }

                            currVehicleX = startingCurrVehicleX;
                            currVehicleY += buttonHeight + 10f;
                        }
                        else
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "No sunroof mounted.", GUIRenderer.labelStyle);
                    }
                    lastHeight = currVehicleY;
                    break;

                case "Material changer":
                    // Increase button width for material changer.
                    buttonWidth *= 1.75f;

                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Material changer", GUIRenderer.headerStyle);
                    currVehicleY += headerHeight;

                    // Find and cache all the parts we can set the material for.
                    if (nextUpdateTime <= 0)
                    {
                        materialParts.Clear();

                        // Add all parts with a condition.
                        foreach (partconditionscript part in carObject.GetComponentsInChildren<partconditionscript>())
                        {
                            materialParts.Add(PartGroup.Create(part.name, part));
                        }

                        // Add any extra conditionless parts.
                        MeshRenderer floor = GameUtilities.GetConditionlessVehiclePartByName(carObject, "Interior");
                        if (floor != null)
                            materialParts.Add(PartGroup.Create("Interior", floor));
                        MeshRenderer floor2 = GameUtilities.GetConditionlessVehiclePartByName(carObject, "Floor");
                        if (floor2 != null)
                            materialParts.Add(PartGroup.Create("Floor", floor2));

                        refreshedCache = true;
                    }

                    // Part selector.
                    string partSelectString = "Select part";
                    if (selectedPart != null)
                        partSelectString = $"Part: {GetPrettyPartName(selectedPart.name)}";
                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), partSelectString))
                        partSelectorOpen = !partSelectorOpen;


                    if (partSelectorOpen)
                    {
                        currVehicleY += buttonHeight;
                        if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "None"))
                        {
                            selectedPart = null;
                            partSelectorOpen = false;
                        }
                        currVehicleY += buttonHeight;
                        foreach (PartGroup group in materialParts)
                        {
                            string parent = group.parts?[0]?.transform.parent?.name;
                            // Hide parent if name matches part name.
                            if (parent != null && GetPrettyPartName(parent) == GetPrettyPartName(group.name))
                                parent = null;
                            if (parent != null)
                                parent = $"(Parent: {GetPrettyPartName(parent)})";
                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), $"{GetPrettyPartName(group.name)} {(parent != null ? parent : "")}"))
                            {
                                selectedPart = group;
                                partSelectorOpen = false;
                            }

                            currVehicleY += buttonHeight + 2f;
                        }

                        currVehicleY += buttonHeight + 10f;
                    }
                    else
                        currVehicleY += buttonHeight + 10f;

                    if (selectedPart != null)
                    {
                        // Material selector.
                        string materialSelectString = "Select material";
                        if (selectedMaterial != null)
                            materialSelectString = $"Material: {materials[selectedMaterial]}";
                        if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), materialSelectString))
                            materialSelectorOpen = !materialSelectorOpen;


                        if (materialSelectorOpen)
                        {
                            currVehicleY += buttonHeight;
                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "None"))
                            {
                                selectedMaterial = null;
                                materialSelectorOpen = false;
                            }
                            currVehicleY += buttonHeight;
                            foreach (KeyValuePair<string, string> material in materials)
                            {
                                if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), material.Value))
                                {
                                    selectedMaterial = material.Key;
                                    materialSelectorOpen = false;
                                }

                                currVehicleY += buttonHeight + 2f;
                            }
                            currVehicleY += buttonHeight + 10f;
                        }
                        else
                            currVehicleY += buttonHeight + 10f;

                        Color? materialColor = null;

                        // Colour selector.
                        if (selectedMaterial != null)
                        {
                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Toggle color selector"))
                                colorSelectorOpen = !colorSelectorOpen;
                            currVehicleY += buttonHeight + 10f;
                        }

                        if (colorSelectorOpen)
                        {
                            // Red.
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Red:", new Color(255, 0, 0)), GUIRenderer.labelStyle);
                            currVehicleY += buttonHeight;
                            float seatRed = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.materialColor.r * 255, 0, 255);
                            seatRed = Mathf.Round(seatRed);
                            currVehicleY += buttonHeight;
                            bool seatRedParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), seatRed.ToString(), GUIRenderer.labelStyle), out seatRed);
                            if (!seatRedParse)
                                Logger.Log($"{seatRedParse} is not a number", Logger.LogLevel.Error);
                            seatRed = Mathf.Clamp(seatRed, 0f, 255f);
                            GUIRenderer.materialColor.r = seatRed / 255f;

                            // Green.
                            currVehicleY += buttonHeight + 10f;
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Green:", new Color(0, 255, 0)), GUIRenderer.labelStyle);
                            currVehicleY += buttonHeight;
                            float seatGreen = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.materialColor.g * 255, 0, 255);
                            seatGreen = Mathf.Round(seatGreen);
                            currVehicleY += buttonHeight;
                            bool seatGreenParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), seatGreen.ToString(), GUIRenderer.labelStyle), out seatGreen);
                            if (!seatGreenParse)
                                Logger.Log($"{seatGreenParse} is not a number", Logger.LogLevel.Error);
                            seatGreen = Mathf.Clamp(seatGreen, 0f, 255f);
                            GUIRenderer.materialColor.g = seatGreen / 255f;

                            // Blue.
                            currVehicleY += buttonHeight + 10f;
                            GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Blue:", new Color(0, 0, 255)), GUIRenderer.labelStyle);
                            currVehicleY += buttonHeight;
                            float seatBlue = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.materialColor.b * 255, 0, 255);
                            seatBlue = Mathf.Round(seatBlue);
                            currVehicleY += buttonHeight;
                            bool seatBlueParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), seatBlue.ToString(), GUIRenderer.labelStyle), out seatBlue);
                            if (!seatBlueParse)
                                Logger.Log($"{seatBlueParse.ToString()} is not a number", Logger.LogLevel.Error);
                            seatBlue = Mathf.Clamp(seatBlue, 0f, 255f);
                            GUIRenderer.materialColor.b = seatBlue / 255f;

                            currVehicleY += buttonHeight + 10f;

                            // Colour preview.
                            // Override alpha for colour preview.
                            Color seatPreview = GUIRenderer.materialColor;
                            seatPreview.a = 1;
                            pixels = new Color[] { seatPreview };
                            previewTexture.SetPixels(pixels);
                            previewTexture.Apply();
                            previewStyle.normal.background = previewTexture;
                            previewStyle.active.background = previewTexture;
                            previewStyle.hover.background = previewTexture;
                            previewStyle.margin = new RectOffset(0, 0, 0, 0);
                            GUI.skin.button = previewStyle;
                            GUI.Button(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), "");
                            GUI.skin.button = defaultStyle;

                            currVehicleY += buttonHeight + 10f;

                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Randomise colour"))
                            {
                                GUIRenderer.materialColor.r = UnityEngine.Random.Range(0f, 255f) / 255f;
                                GUIRenderer.materialColor.g = UnityEngine.Random.Range(0f, 255f) / 255f;
                                GUIRenderer.materialColor.b = UnityEngine.Random.Range(0f, 255f) / 255f;
                            }

                            currVehicleY += buttonHeight + 10f;

                            GUIRenderer.materialColor = GUIRenderer.RenderColourPalette(currVehicleX, currVehicleY, sliderWidth + 20f, GUIRenderer.materialColor);
                            currVehicleY += GUIRenderer.GetPaletteHeight(sliderWidth + 20f) + 10f;

                            materialColor = GUIRenderer.materialColor;
                        }

                        if (selectedMaterial != null && GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply"))
                        {
                            if (selectedPart.IsConditionless())
                            {
                                foreach (MeshRenderer mesh in selectedPart.meshes)
                                {
                                    Thread thread = new Thread(() =>
                                    {
                                        GameUtilities.SetConditionlessPartMaterial(mesh, selectedMaterial, materialColor);
                                        SaveUtilities.UpdateMaterials(new MaterialData()
                                        {
                                            ID = save.idInSave,
                                            part = selectedPart.name,
                                            isConditionless = true,
                                            exact = IsExact(selectedPart.name),
                                            type = selectedMaterial,
                                            color = materialColor
                                        });
                                    });
                                    thread.Start();
                                }
                            }
                            else
                            {
                                foreach (partconditionscript part in selectedPart.parts)
                                {
                                    Thread thread = new Thread(() =>
                                    {
                                        GameUtilities.SetPartMaterial(part, selectedMaterial, materialColor);
                                        SaveUtilities.UpdateMaterials(new MaterialData()
                                        {
                                            ID = save.idInSave,
                                            part = selectedPart.name,
                                            exact = IsExact(selectedPart.name),
                                            type = selectedMaterial,
                                            color = materialColor
                                        });
                                    });
                                    thread.Start();
                                }
                            }
                        }
                    }
                    lastHeight = currVehicleY;
                    break;

                case "Randomised changer":
                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Randomiser changer", GUIRenderer.headerStyle);
                    currVehicleY += headerHeight;

                    // Randomised part selector.
                    if (nextUpdateTime <= 0)
                    {
                        randomParts.Clear();
                        randomParts = carObject.GetComponentsInChildren<randomTypeSelector>().ToList();

                        refreshedCache = true;
                    }

                    // Random selector.
                    string randomSelectString = "Select randomised part";
                    if (selectedRandom != null)
                        randomSelectString = $"Selected: {PrettifyName(selectedRandom.name)}";
                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), randomSelectString))
                        randomSelectorOpen = !randomSelectorOpen;

                    currVehicleY += buttonHeight + 10f;

                    if (randomSelectorOpen)
                    {
                        if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "None"))
                        {
                            selectedRandom = null;
                            randomSelectorOpen = false;
                        }
                        currVehicleY += buttonHeight + 2f;
                        foreach (randomTypeSelector random in randomParts)
                        {
                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), PrettifyName(random.name)))
                            {
                                selectedRandom = random;
                                randomSelectorOpen = false;
                            }
                            currVehicleY += buttonHeight + 2f;
                        }
                        currVehicleY += buttonHeight + 10f;
                    }

                    if (selectedRandom != null)
                    {
                        for (int i = 0; i < selectedRandom.tipusok.Length; i++)
                        {
                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), $"Option {i + 1}"))
                            {
                                selectedRandom.rtipus = i;
                                selectedRandom.Refresh();
                            }
                            currVehicleY += buttonHeight + 2f;
                        }
                    }
                    lastHeight = currVehicleY;
                    break;

                case "Light changer":
                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Light changer", GUIRenderer.headerStyle);
                    currVehicleY += headerHeight;

                    // Light changer.
                    if (nextUpdateTime <= 0)
                    {
                        lights.Clear();
                        headlightscript[] headlights = carObject.GetComponentsInChildren<headlightscript>();
                        if (headlights.Length > 0)
                        {
                            for (int i = 0; i < headlights.Length; i++)
                            {
                                headlightscript headlight = headlights[i];
                                string name = $"{i + 1} - Headlight";
                                bool isInterior = false;
                                if (headlight.name.ToLower().Contains("interior") || headlight.transform.parent.name.ToLower().Contains("interior"))
                                {
                                    name = $"{i + 1} - Interior light";
                                    isInterior = true;
                                }
                                lights.Add(LightGroup.Create(name, headlight, isInterior));
                            }
                        }

                        refreshedCache = true;
                    }

                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Choose lights to alter", GUIRenderer.subHeaderStyle);
                    currVehicleY += headerHeight;

                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Select"))
                        lightSelectorOpen = !lightSelectorOpen;

                    currVehicleY += buttonHeight;

                    if (lightSelectorOpen)
                    {
                        foreach (LightGroup light in lights)
                        {
                            // Remove selected lights from selectable.
                            if (selectedLights.Where(l => l.name == light.name).FirstOrDefault() != null) continue;

                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), PrettifyName(light.name)))
                                selectedLights.Add(light);
                            currVehicleY += buttonHeight + 2f;
                        }
                        currVehicleY += buttonHeight + 10f;
                    }

                    currVehicleY += buttonHeight + 10f;
                    GUI.Label(new Rect(currVehicleX, currVehicleY, headerWidth, headerHeight), "Selected lights", GUIRenderer.subHeaderStyle);
                    currVehicleY += headerHeight;

                    if (selectedLights.Count == 0)
                    {
                        GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Nothing selected");
                    }
                    else
                    {
                        foreach (LightGroup light in selectedLights)
                        {
                            if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), PrettifyName(light.name)))
                            {
                                selectedLights.Remove(light);
                                break;
                            }
                            currVehicleY += buttonHeight + 2f;
                        }
                    }
                    currVehicleY += buttonHeight + 10f;

                    // Red.
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Red:", new Color(255, 0, 0)), GUIRenderer.labelStyle);
                    currVehicleY += buttonHeight;
                    float lightRed = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.lightColor.r * 255, 0, 255);
                    lightRed = Mathf.Round(lightRed);
                    currVehicleY += buttonHeight;
                    bool lightRedParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), lightRed.ToString(), GUIRenderer.labelStyle), out lightRed);
                    if (!lightRedParse)
                        Logger.Log($"{lightRedParse} is not a number", Logger.LogLevel.Error);
                    lightRed = Mathf.Clamp(lightRed, 0f, 255f);
                    GUIRenderer.lightColor.r = lightRed / 255f;

                    // Green.
                    currVehicleY += buttonHeight + 10f;
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Green:", new Color(0, 255, 0)), GUIRenderer.labelStyle);
                    currVehicleY += buttonHeight;
                    float lightGreen = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.lightColor.g * 255, 0, 255);
                    lightGreen = Mathf.Round(lightGreen);
                    currVehicleY += buttonHeight;
                    bool lightGreenParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), lightGreen.ToString(), GUIRenderer.labelStyle), out lightGreen);
                    if (!lightGreenParse)
                        Logger.Log($"{lightGreenParse} is not a number", Logger.LogLevel.Error);
                    lightGreen = Mathf.Clamp(lightGreen, 0f, 255f);
                    GUIRenderer.lightColor.g = lightGreen / 255f;

                    // Blue.
                    currVehicleY += buttonHeight + 10f;
                    GUI.Label(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), GUIRenderer.GetAccessibleColorString("Blue:", new Color(0, 0, 255)), GUIRenderer.labelStyle);
                    currVehicleY += buttonHeight;
                    float lightBlue = GUI.HorizontalSlider(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), GUIRenderer.lightColor.b * 255, 0, 255);
                    lightBlue = Mathf.Round(lightBlue);
                    currVehicleY += buttonHeight;
                    bool lightBlueParse = float.TryParse(GUI.TextField(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), lightBlue.ToString(), GUIRenderer.labelStyle), out lightBlue);
                    if (!lightBlueParse)
                        Logger.Log($"{lightBlueParse.ToString()} is not a number", Logger.LogLevel.Error);
                    lightBlue = Mathf.Clamp(lightBlue, 0f, 255f);
                    GUIRenderer.lightColor.b = lightBlue / 255f;

                    currVehicleY += buttonHeight + 10f;

                    // Colour preview.
                    // Override alpha for colour preview.
                    Color lightPreview = GUIRenderer.lightColor;
                    lightPreview.a = 1;
                    pixels = new Color[] { lightPreview };
                    previewTexture.SetPixels(pixels);
                    previewTexture.Apply();
                    previewStyle.normal.background = previewTexture;
                    previewStyle.active.background = previewTexture;
                    previewStyle.hover.background = previewTexture;
                    previewStyle.margin = new RectOffset(0, 0, 0, 0);
                    GUI.skin.button = previewStyle;
                    GUI.Button(new Rect(currVehicleX, currVehicleY, sliderWidth, buttonHeight), "");
                    GUI.skin.button = defaultStyle;

                    currVehicleY += buttonHeight + 10f;

                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Randomise colour"))
                    {
                        GUIRenderer.lightColor.r = UnityEngine.Random.Range(0f, 255f) / 255f;
                        GUIRenderer.lightColor.g = UnityEngine.Random.Range(0f, 255f) / 255f;
                        GUIRenderer.lightColor.b = UnityEngine.Random.Range(0f, 255f) / 255f;
                    }

                    currVehicleY += buttonHeight + 10f;

                    GUIRenderer.lightColor = GUIRenderer.RenderColourPalette(currVehicleX, currVehicleY, sliderWidth + 20f, GUIRenderer.lightColor);
                    currVehicleY += GUIRenderer.GetPaletteHeight(sliderWidth + 20f) + 10f;

                    if (GUI.Button(new Rect(currVehicleX, currVehicleY, buttonWidth, buttonHeight), "Apply to selected"))
                    {
                        foreach (LightGroup light in selectedLights)
                        {
                            if (light.headlights != null && light.headlights.Count > 0)
                            {
                                foreach (headlightscript headlight in light.headlights)
                                {
                                    GameUtilities.SetHeadlightColor(headlight, GUIRenderer.lightColor, light.isInteriorLight);
                                    int? id = save.idInSave;
                                    if (!light.isInteriorLight)
                                        id = headlight.GetComponent<tosaveitemscript>()?.idInSave;

                                    string name = null;
                                    if (light.isInteriorLight)
                                        name = "interior";

                                    if (id.HasValue)
                                        SaveUtilities.UpdateLight(new LightData() { ID = id.Value, name = name, color = GUIRenderer.lightColor });
                                }
                            }
                        }
                    }
                    lastHeight = currVehicleY;
                    break;     
            }

            GUI.EndScrollView();

            lastCarId = save.idInSave;

            // Cache has been refreshed, set the next cache refresh time.
            if (refreshedCache)
				nextUpdateTime = updateFrequency;
		}

		/// <summary>
		/// Make part name more user friendly.
		/// </summary>
		/// <param name="part">Part name to translate</param>
		/// <returns>Prettified part name</returns>
		private string GetPrettyPartName(string part)
		{
            part = PrettifyName(part);

            switch (part)
			{
				case "PartConColorLeather":
					return "Main seats";
				case "NapellenzoLeft":
					return "Left sun visor";
				case "NapellenzoRight":
					return "Right sun visor";
                case "GloveStore":
                    return "Glove box";
                case "Karpit":
					return "Headliner";
				case "PartConCsik":
					return "Fury stripe";
                case "PartConMetal":
                    return "Metal";
                case "PartConMetals":
                    return "Metals";
                case "PartConMetals2":
                    return "Metals 2";
                case "PartConLeather":
                    return "Leather";
                case "PartConDarkMetal":
                    return "Dark metals";
                case "PartConConv":
                    return "Soft top roof";
                case "PartConCar03Karpit":
                    return "Beetle shelf";
                case "Interior":
                case "Floor":
                    return "Carpet";
            }

            string partLower = part.ToLower();

			if (partLower.Contains("seat"))
				return "Removable seat";

            if (partLower.Contains("felni"))
                return "Wheel";

            if (partLower.Contains("gumi"))
                return "Tire";

            if (partLower.Contains("disztarcsa"))
                return "Hubcap";

            if (partLower.Contains("coolant"))
                return "Radiator";

            if (partLower.Contains("kesztyutarto"))
                return "Glove box";

			return part;
		}

		/// <summary>
		/// Whether the part matches by exact name.
		/// </summary>
		/// <param name="part">Part name to check</param>
		/// <returns>Returns true if the part matches off exact name, otherwise false.</returns>
		private bool IsExact(string part)
		{
			switch (part)
			{
				case "PartConColorLeather":
				case "NapellenzoLeft":
				case "NapellenzoRight":
					return true;
			}
			return false;
		}

		/// <summary>
		/// Make part name more user friendly.
		/// </summary>
		/// <param name="random">Part name to prettify</param>
		/// <returns>Prettified part name</returns>
		private string PrettifyName(string name)
		{
            return name.Replace("(Clone)", string.Empty);
		}
	}
}
