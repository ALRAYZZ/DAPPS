using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System.Numerics;
using Windows.UI;

namespace DAPPSApp.Controls
{
	public class DropShadowPanel : ContentControl
	{
		private SpriteVisual _shadowVisual;
		private Compositor _compositor;

		public DropShadowPanel()
		{
			// Subscribe to the Loaded event
			this.Loaded += OnLoaded;

			// Subscribe to SizeChanged to update the shadow size dynamically
			this.SizeChanged += OnSizeChanged;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (Window.Current != null && Window.Current.Compositor != null)
			{
				_compositor = Window.Current.Compositor;

				// Create the shadow visual
				_shadowVisual = _compositor.CreateSpriteVisual();
				_shadowVisual.Size = new Vector2((float)this.ActualWidth, (float)this.ActualHeight);
				_shadowVisual.Offset = new Vector3(0, 0, 0);

				// Create the drop shadow
				var shadow = _compositor.CreateDropShadow();
				shadow.Color = ShadowColor;
				shadow.BlurRadius = BlurRadius;
				shadow.Opacity = ShadowOpacity;
				shadow.Offset = new Vector3(OffsetX, OffsetY, 0);

				_shadowVisual.Shadow = shadow;

				// Attach the shadow visual to the control
				ElementCompositionPreview.SetElementChildVisual(this, _shadowVisual);
			}
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateShadow();
		}

		private void UpdateShadow()
		{
			if (_shadowVisual != null && _compositor != null)
			{
				var shadow = _shadowVisual.Shadow as DropShadow;
				if (shadow != null)
				{
					shadow.Color = ShadowColor;
					shadow.BlurRadius = BlurRadius;
					shadow.Opacity = ShadowOpacity;
					shadow.Offset = new Vector3(OffsetX, OffsetY, 0);

					// Update the size of the shadow visual
					_shadowVisual.Size = new Vector2((float)this.ActualWidth, (float)this.ActualHeight);
				}
			}
		}

		#region Dependency Properties

		public Color ShadowColor
		{
			get => (Color)GetValue(ShadowColorProperty);
			set => SetValue(ShadowColorProperty, value);
		}

		public static readonly DependencyProperty ShadowColorProperty =
			DependencyProperty.Register(
				"ShadowColor",
				typeof(Color),
				typeof(DropShadowPanel),
				new PropertyMetadata(Colors.Black, OnShadowColorChanged));

		private static void OnShadowColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is DropShadowPanel panel)
			{
				panel.UpdateShadow();
			}
		}

		public float BlurRadius
		{
			get => (float)GetValue(BlurRadiusProperty);
			set => SetValue(BlurRadiusProperty, value);
		}

		public static readonly DependencyProperty BlurRadiusProperty =
			DependencyProperty.Register(
				"BlurRadius",
				typeof(float),
				typeof(DropShadowPanel),
				new PropertyMetadata(10f, OnBlurRadiusChanged));

		private static void OnBlurRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is DropShadowPanel panel)
			{
				panel.UpdateShadow();
			}
		}

		public float ShadowOpacity
		{
			get => (float)GetValue(ShadowOpacityProperty);
			set => SetValue(ShadowOpacityProperty, value);
		}

		public static readonly DependencyProperty ShadowOpacityProperty =
			DependencyProperty.Register(
				"ShadowOpacity",
				typeof(float),
				typeof(DropShadowPanel),
				new PropertyMetadata(0.5f, OnShadowOpacityChanged));

		private static void OnShadowOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is DropShadowPanel panel)
			{
				panel.UpdateShadow();
			}
		}

		public float OffsetX
		{
			get => (float)GetValue(OffsetXProperty);
			set => SetValue(OffsetXProperty, value);
		}

		public static readonly DependencyProperty OffsetXProperty =
			DependencyProperty.Register(
				"OffsetX",
				typeof(float),
				typeof(DropShadowPanel),
				new PropertyMetadata(0f, OnOffsetChanged));

		public float OffsetY
		{
			get => (float)GetValue(OffsetYProperty);
			set => SetValue(OffsetYProperty, value);
		}

		public static readonly DependencyProperty OffsetYProperty =
			DependencyProperty.Register(
				"OffsetY",
				typeof(float),
				typeof(DropShadowPanel),
				new PropertyMetadata(0f, OnOffsetChanged));

		private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is DropShadowPanel panel)
			{
				panel.UpdateShadow();
			}
		}

		#endregion
	}
}


