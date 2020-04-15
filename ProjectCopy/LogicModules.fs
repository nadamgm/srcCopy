namespace SourceExtractor.LogicModules

open System.IO

type ProgramConfiguration = {OriginProjectPath: string; TargetPath: string;  TargetNameSpace: string}
type FileSystemItem = | Dir of string | SingleFile of string
type FileSystemTree = | Node of FileSystemItem | Branch of FileSystemItem * FileSystemTree seq

module OriginCrawler = 

    let private createNewDirStructure (config: ProgramConfiguration ) (input: string seq)=  let subFoldersAndFiles = input |> Seq.map(fun x -> x.Substring(config.OriginProjectPath.Length))
                                                                                            subFoldersAndFiles |> Seq.map(fun x -> sprintf "%s\\%s" config.TargetPath x)


    let rec FromDirectory (dirInfo: string) cfg = 
                                                 let subItems = 
                                                        seq{
                                                            yield! Directory.EnumerateFiles(dirInfo, "*.cs") |> createNewDirStructure cfg |> Seq.map(fun x -> Node(SingleFile(x)))
                                                            yield! Directory.EnumerateDirectories(dirInfo) |> Seq.map( fun x -> FromDirectory x cfg)
                                                            }
                                                 Branch (Dir(dirInfo), subItems)


    let ReplaceNameSpace cfg filepath = let fileContent = File.ReadAllText filepath
                                        let projectFileName = Directory.GetFiles(cfg.OriginProjectPath, "*.csproj").[0].Split('.').[0]
                                        let newContent = fileContent.Replace(projectFileName, cfg.TargetNameSpace)
                                        File.WriteAllText(filepath, newContent)

    let rec CopyProjectFiles (tree: FileSystemTree) cfg = match tree with 
                                                                        | Node n -> match n with | SingleFile f -> ReplaceNameSpace cfg f
                                                                        | Branch (directory, contents) -> Directory.CreateDirectory(directory) |> ignore    

    
                                               