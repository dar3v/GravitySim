using Godot;

public static class SplashLayout
{
	public static void Apply(Control root)
	{
		SetFullRect(root);

		var bg = root.GetNode<ColorRect>("ColorRect");
		SetFullRect(bg);
		bg.Color = new Color("#242424");


		var center = bg.GetNode<CenterContainer>("CenterContainer");
		SetFullRect(center);

		var logo = center.GetNode<TextureRect>("TextureRect");
		logo.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		logo.CustomMinimumSize = new Vector2(250, 250);
		logo.Texture = GD.Load<Texture2D>("res://Assets/icon.svg");

		var bar = bg.GetNode<ProgressBar>("ProgressBar");
		bar.AnchorLeft = 0.25f;
		bar.AnchorRight = 0.75f;
		bar.AnchorTop = 0.8f;
		bar.AnchorBottom = 0.8f;

		bar.OffsetTop = 0;
		bar.OffsetBottom = 16;
		bar.Value = 0;
		bar.MaxValue = 100;
	}

	private static void SetFullRect(Control c)
	{
		c.AnchorLeft = 0;
		c.AnchorTop = 0;
		c.AnchorRight = 1;
		c.AnchorBottom = 1;
		c.OffsetLeft = c.OffsetTop = c.OffsetRight = c.OffsetBottom = 0;
	}
}
