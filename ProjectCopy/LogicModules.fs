namespace SourceExtractor.LogicModules

open System.IO
open System.Diagnostics
open System.Linq

type ProgramConfiguration = {OriginProjectPath: string; TargetPath: string;  TargetNameSpace: string}
type FileSysTree = | FileUnit of string | Folder of string * FileSysTree list

module OriginCrawler = 

    let private createNewDirStructure (config: ProgramConfiguration ) (input: string seq)=  let subFoldersAndFiles = input |> Seq.map(fun x -> x.Substring(config.OriginProjectPath.Length))
                                                                                            subFoldersAndFiles |> Seq.map(fun x -> sprintf "%s\\%s" config.TargetPath x)

    let private remapStrings cfg (input:string) = let cut = input.Substring(cfg.OriginProjectPath.Length)
                                                  if cut = "" then cfg.TargetPath
                                                  else
                                                  sprintf "%s\\%s" cfg.TargetPath cut

    let rec correctDirPath (config: ProgramConfiguration) (input: FileSysTree) = match input with | FileUnit n -> remapStrings config n |> FileUnit
                                                                                                  | Folder (name, contents) -> let remap = remapStrings config name
                                                                                                                               let children = List.map(correctDirPath config) contents
                                                                                                                               Folder(remap, children)

    let rec CrawlFileSystemTree (dirInfo: string) cfg = 
                                                        let subItems = 
                                                               seq{
                                                                   yield! Directory.EnumerateFiles(dirInfo, "*.cs") |> Seq.map(FileUnit)
                                                                   yield! Directory.EnumerateDirectories(dirInfo) |> Seq.map( fun x -> CrawlFileSystemTree x cfg)
                                                                   }
                                                        Folder (dirInfo, subItems |> List.ofSeq)



    let ReplaceNameSpace cfg filepath = let fileContent = File.ReadAllText filepath
                                        let projectFileName = Directory.GetFiles(cfg.OriginProjectPath, "*.csproj").[0].Split('.').[0].Split('\\').Last()
                                        let newContent = fileContent.Replace(projectFileName, cfg.TargetNameSpace)
                                        File.WriteAllText(remapStrings cfg filepath, newContent)

  
    let rec CopyProjectFiles cfg (tree: FileSysTree) = 
                                                        match tree with | FileUnit f -> ReplaceNameSpace cfg f
                                                                        | Folder (name, content) -> remapStrings cfg name |> Directory.CreateDirectory |> ignore
                                                                                                    List.map (CopyProjectFiles cfg) content |> ignore


module CommandRunner = 

    
    let RunDotnetProcess (cfg: ProgramConfiguration) =  
                                                        use processRunner = new Process()
                                                        processRunner.StartInfo.FileName <- "dotnet"
                                                        Directory.CreateDirectory cfg.TargetPath |> ignore
                                                        processRunner.StartInfo.Arguments <- sprintf "new console -n %s -o %s" cfg.TargetNameSpace cfg.TargetPath
                                                        processRunner.Start() |> ignore                                                        
                                                        processRunner.WaitForExit()

    //let Run cfg = setupDotnetProcess(cfg).Start()
    
                                               