#load code/const.cake
#load code/info.cake
#load code/build-task.cake

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var actions = Argument<string>("action").Split('|').Select(x => x.Trim()).ToArray();


///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
.Does(() => {
   Information("Hello Cake!");
});

RunTarget("Info");

foreach (var action in actions) {
   string taskName;
   
   if (action == "Info") continue; 
   else if (action == "Build") taskName = $"Build-{Global.Platform}";
   // else if (action == "Bundle") taskName = $"Bundle-{Global.Platform}";
   else if (action == "Validation") taskName = $"Validation-{Global.Platform}";
   // else if (action == "Publish") taskName = $"Publish-{BuildConfig.Var("Publish")}"; 
   else if (action.ToLowerInvariant().StartsWith("raw:")) taskName = action[4..]; 
   else throw new Exception($"Unknown action: '{action}'");

   Information($"-----------------------------------------------");
   Information($"{action}");
   Information($"-----------------------------------------------");

   RunTarget(taskName);
}
