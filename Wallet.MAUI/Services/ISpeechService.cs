using System;
namespace Wallet.MAUI.Services
{
	public interface ISpeechService
	{
		Task<string> ProcessSpeech();
	}
}

