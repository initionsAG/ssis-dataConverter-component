using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Allgemeine Informationen über eine Assembly werden über die folgenden 
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die mit einer Assembly verknüpft sind.
[assembly: AssemblyTitle("DataConverter")]
#if     (SQL2008)
[assembly: AssemblyDescription("for Integration Services 2008")]
#elif   (SQL2012)
[assembly: AssemblyDescription("for Integration Services 2012")]
#elif   (SQL2014)
[assembly: AssemblyDescription("for Integration Services 2014")]
#else
[assembly: AssemblyDescription("for Integration Services 2008")]
#endif
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("innovative IT solutions AG")]
[assembly: AssemblyProduct("DataConverter")]
[assembly: AssemblyCopyright("Copyright © initions AG 2011-2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Durch Festlegen von ComVisible auf "false" werden die Typen in dieser Assembly unsichtbar 
// für COM-Komponenten. Wenn Sie auf einen Typ in dieser Assembly von 
// COM zugreifen müssen, legen Sie das ComVisible-Attribut für diesen Typ auf "true" fest.
[assembly: ComVisible(false)]

// Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird


#if     (Sql2008)
[assembly: Guid("1c59d890-87bb-4940-bb04-072391c53ef4")]
#elif   (SQL2012)
[assembly: Guid("996889B2-F686-4DBA-9073-C94B33C500E2")]
#elif   (SQL2014)
[assembly: Guid("FBF9FC4C-26CF-4CC1-B037-75EF7D66B633")]
#else
[assembly: Guid("1c59d890-87bb-4940-bb04-072391c53ef4")]
#endif

// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion 
//      Buildnummer
//      Revision
//
// Sie können alle Werte angeben oder die standardmäßigen Build- und Revisionsnummern 
// übernehmen, indem Sie "*" eingeben:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.1.10.0")]
