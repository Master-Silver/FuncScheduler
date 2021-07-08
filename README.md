# FuncScheduler 
The FuncScheduler helps to run an action or asynchronous function at a given time.

He uses only one instance from the [Timer](https://docs.microsoft.com/de-de/dotnet/api/system.timers.timer?view=net-5.0) class from Microsoft to schedule him self. Because the Timer class is event base, there is no performance penalty, but there is a queuing and sorting overhead when adding a new action or function. If the set time is elapsed or reached the action or function will be queued to run on the Threadpool.

Adding actions or functions on the FuncScheduler is thread save and there are some unit tests which are explicitly testing this. 

You can run the _Example_ console project to see how it works.

If you like to improve the FuncScheduler or encounter a problem just let me know.