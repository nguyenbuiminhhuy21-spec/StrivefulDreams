using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Code_Game.Scripts.Screens.CharacterCreation;

public class CharacterCreationLayoutData
{
    public ElementConfig Background { get; set; }
    public ElementConfig Panel { get; set; }
    public ElementConfig Frame { get; set; }
    public AppearanceSelectorsConfig AppearanceSelectors { get; set; }
    public IdentitySelectorsConfig IdentitySelectors { get; set; }
    public FarmTypeSelectorConfig FarmTypeSelector { get; set; }
    public DatePickerConfig DatePicker { get; set; }
    public Dictionary<string, UIElementConfig> Buttons { get; set; }

    public class AppearanceSelectorsConfig
    {
        public UIElementConfig Common { get; set; }
        public UIElementConfig Hair { get; set; }
        public UIElementConfig Shirt { get; set; }
        public UIElementConfig Pants { get; set; }

        public void ApplyCommon()
        {
            if (Common == null) return;
            Apply(Hair);
            Apply(Shirt);
            Apply(Pants);
        }

        private void Apply(UIElementConfig target)
        {
            if (target == null) return;
            target.RelativeTo ??= Common.RelativeTo;
            if (target.Scale == null) target.Scale = Common.Scale;
            if (target.OriginX == null) target.OriginX = Common.OriginX;
            if (target.OriginY == null) target.OriginY = Common.OriginY;
            if (target.PaddingX == null) target.PaddingX = Common.PaddingX;
            if (target.PaddingY == null) target.PaddingY = Common.PaddingY;
            if (target.Wheel == null) target.Wheel = Common.Wheel;
            if (target.Bar == null) target.Bar = Common.Bar;
            if (target.LabelSettings == null) target.LabelSettings = Common.LabelSettings;
            if (target.Controls == null) target.Controls = Common.Controls;
            if (target.DescriptionSettings == null) target.DescriptionSettings = Common.DescriptionSettings;
        }
    }

    public class IdentitySelectorsConfig
    {
        public UIElementConfig Common { get; set; }
        public UIElementConfig Input1 { get; set; }
        public UIElementConfig Input2 { get; set; }
        public UIElementConfig Select1 { get; set; }

        public void ApplyCommon()
        {
            if (Common == null) return;
            Apply(Input1);
            Apply(Input2);
            Apply(Select1);
        }

        private void Apply(UIElementConfig target)
        {
            if (target == null) return;
            target.RelativeTo ??= Common.RelativeTo;
            if (target.Scale == null) target.Scale = Common.Scale;
            if (target.OriginX == null) target.OriginX = Common.OriginX;
            if (target.OriginY == null) target.OriginY = Common.OriginY;
            if (target.PaddingX == null) target.PaddingX = Common.PaddingX;
            if (target.PaddingY == null) target.PaddingY = Common.PaddingY;
            if (target.LabelSettings == null) target.LabelSettings = Common.LabelSettings;
            if (target.DescriptionSettings == null) target.DescriptionSettings = Common.DescriptionSettings;
        }
    }

    public class UIElementConfig : ElementConfig
    {
        public string Label { get; set; }
        public WheelConfig Wheel { get; set; }
        public BarConfig Bar { get; set; }
        public LabelConfig LabelSettings { get; set; }
        public ControlsConfig Controls { get; set; }
        public DescriptionConfig DescriptionSettings { get; set; }
        public string Placeholder { get; set; }
        public List<string> Options { get; set; }
    }

    public class ElementConfig
    {
        public string RelativeTo { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public float? Scale { get; set; }
        public float? OriginX { get; set; }
        public float? OriginY { get; set; }
        public float? PaddingX { get; set; }
        public float? PaddingY { get; set; }

        public float GetX() => X ?? 0;
        public float GetY() => Y ?? 0;
        public float GetWidth() => Width ?? 0;
        public float GetHeight() => Height ?? 0;
        public float GetScale() => Scale ?? 1.0f;
        public float GetOriginX() => OriginX ?? 0;
        public float GetOriginY() => OriginY ?? 0;
        public float GetPaddingX() => PaddingX ?? 0;
        public float GetPaddingY() => PaddingY ?? 0;
    }

    public class WheelConfig
    {
        public float? Scale { get; set; }
        public float? OffsetX { get; set; }
        public float? OffsetY { get; set; }
        public float? BorderWidth { get; set; }
        public string BorderColor { get; set; }
        public HandleConfig Handle { get; set; }
        public float GetScale() => Scale ?? 1.0f;
        public float GetOffsetX() => OffsetX ?? 0;
        public float GetOffsetY() => OffsetY ?? 0;
        public float GetBorderWidth() => BorderWidth ?? 0;
    }

    public class BarConfig
    {
        public float? Width { get; set; }
        public float? OffsetX { get; set; }
        public float? OffsetY { get; set; }
        public float? Radius { get; set; }
        public float? BorderWidth { get; set; }
        public string BorderColor { get; set; }
        public HandleConfig Handle { get; set; }
        public float GetWidth() => Width ?? 20;
        public float GetOffsetX() => OffsetX ?? 0;
        public float GetOffsetY() => OffsetY ?? 0;
        public float GetRadius() => Radius ?? 0;
        public float GetBorderWidth() => BorderWidth ?? 0;
    }

    public class HandleConfig
    {
        public float? Size { get; set; }
        public string Color { get; set; }
        public string BorderColor { get; set; }
        public float? BorderWidth { get; set; }
        public float GetSize() => Size ?? 10;
        public float GetBorderWidth() => BorderWidth ?? 1;
    }

    public class LabelConfig
    {
        public float? FontScale { get; set; }
        public float? OffsetX { get; set; }
        public float? OffsetY { get; set; }
        public string TextColor { get; set; }
        public float GetFontScale() => FontScale ?? 1.0f;
        public float GetOffsetX() => OffsetX ?? 0;
        public float GetOffsetY() => OffsetY ?? 0;
    }

    public class ControlsConfig
    {
        public float? ButtonSize { get; set; }
        public float? SpacingX { get; set; }
        public float? RowOffsetY { get; set; }
        public float? ColorIconOffsetX { get; set; }
        public float? ModalOffsetX { get; set; }
        public float? ModalOffsetY { get; set; }

        public float GetButtonSize() => ButtonSize ?? 32;
        public float GetSpacingX() => SpacingX ?? 10;
        public float GetRowOffsetY() => RowOffsetY ?? 30;
        public float GetColorIconOffsetX() => ColorIconOffsetX ?? 20;
        public float GetModalOffsetX() => ModalOffsetX ?? 20;
        public float GetModalOffsetY() => ModalOffsetY ?? 0;
    }

    public class DescriptionConfig
    {
        public float IconX { get; set; } = 20;
        public float IconY { get; set; } = 20;
        public float IconScale { get; set; } = 1.0f;
        public float TitleX { get; set; } = 80;
        public float TitleY { get; set; } = 20;
        public float TitleScale { get; set; } = 1.0f;
        public float ContentX { get; set; } = 20;
        public float ContentY { get; set; } = 80;
        public float ContentScale { get; set; } = 0.85f;
        public int FrameWidth { get; set; } = 320;
        public int FrameHeight { get; set; } = 220;
    }

    public class FarmTypeSelectorConfig : UIElementConfig
    {
        public string Title { get; set; }
        public new List<FarmTypeOption> Options { get; set; }
        public GridConfig Grid { get; set; }
    }

    public class GridConfig
    {
        public int Columns { get; set; } = 1;
        public float SpacingX { get; set; } = 10;
        public float SpacingY { get; set; } = 10;
        public float ItemSize { get; set; } = 64;
    }

    public class FarmTypeOption
    {
        public string Key { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public string Texture { get; set; }
    }

    public class DatePickerConfig : UIElementConfig
    {
        public SelectorConfig SeasonSelector { get; set; }
        public SelectorConfig DaySelector { get; set; }

        public class SelectorConfig
        {
            public float Width { get; set; }
            public float OffsetX { get; set; }
            public float? IconOffsetX { get; set; }
            public float? IconOffsetY { get; set; }
            public float? IconScale { get; set; }
            public float? IconPadding { get; set; }
            public bool? StretchIcon { get; set; }
            public float? ArrowOffsetX { get; set; }
            public float? ArrowOffsetY { get; set; }
            public float? ArrowScale { get; set; }
        }
    }
}
