﻿//      *********    DO NOT MODIFY THIS FILE     *********
//      This file is regenerated by a design tool. Making
//      changes to this file can cause errors.
namespace Expression.Blend.SampleData.SampleDataSource
{
	using System; 
	using System.ComponentModel;

// To significantly reduce the sample data footprint in your production application, you can set
// the DISABLE_SAMPLE_DATA conditional compilation constant and disable sample data at runtime.
#if DISABLE_SAMPLE_DATA
	internal class SampleDataSource { }
#else

	public class SampleDataSource : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public SampleDataSource()
		{
			try
			{
				Uri resourceUri = new Uri("/TwitchAlert;component/SampleData/SampleDataSource/SampleDataSource.xaml", UriKind.RelativeOrAbsolute);
				System.Windows.Application.LoadComponent(this, resourceUri);
			}
			catch
			{
			}
		}

		private string _Game = string.Empty;

		public string Game
		{
			get
			{
				return this._Game;
			}

			set
			{
				if (this._Game != value)
				{
					this._Game = value;
					this.OnPropertyChanged("Game");
				}
			}
		}

		private string _Status = string.Empty;

		public string Status
		{
			get
			{
				return this._Status;
			}

			set
			{
				if (this._Status != value)
				{
					this._Status = value;
					this.OnPropertyChanged("Status");
				}
			}
		}

		private string _DisplayName = string.Empty;

		public string DisplayName
		{
			get
			{
				return this._DisplayName;
			}

			set
			{
				if (this._DisplayName != value)
				{
					this._DisplayName = value;
					this.OnPropertyChanged("DisplayName");
				}
			}
		}
	}
#endif
}