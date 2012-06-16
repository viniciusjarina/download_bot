![logo](http://files.softicons.com/download/tv-movie-icons/futurama-icons-by-rich-d/png/128/Bender.png)

Download Bot
=====================
Features
--------------
* Download a set of files using a pattern match
* Using ranges (integers, dates)

Usage
-----

	download_bot url start,end [folder]
	
Exemple:

	#>mono download_bot.exe http://server/files/{0}.jpg 10;100
	
Will downloading http://server/files/10.jpg, http://server/files/11.jpg, http://server/files/12.jpg … http://server/files/100.jpg


