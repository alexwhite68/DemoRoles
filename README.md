# DemoRoles

I really struggled to find one project that was built using the latest .Net 5 Framework,
that was Blazor Web Assembly, using the built in security system, using SQLite rather than SQL Server,
I needed to get roles, policies working specifically multiple roles per user was a requirement.

This project covers all those topics.

Key points, using a web assembly you have to find a way to get the user tokens onto the client, decoded 
so they are usable, e.g. roles/policies, I have tried to make as little modifications to a standard project
as possible to get this working, policies are defined in the shared project so they are available to 
both the client and server.

Gotchas, more than one role, the standard system will push multiple roles down as one, they need to be 
decoded so you can see the individual roles, look in the root of the client project for the code to do this.
that code comes from the following project https://github.com/cradle77/BlazorSecurityDemo blog for the code
https://medium.com/@marcodesanctis2/securing-blazor-webassembly-with-identity-server-4-ee44aa1687ef

Feel free to copy and use as you wish, I hope it saves you days of not banging your head against a brickwall like me!.




