// Learn more about F# at http://fsharp.org

open System.IO
open SourceExtractor.LogicModules

let rec ArgumentParser state (args: string seq)= let input = List.ofSeq args
                                                 match input with | [] -> state
                                                                  | "-o"::ls -> let newState = {state with OriginProjectPath = ls.Head} 
                                                                                ArgumentParser newState ls.Tail
                                                                  | "-t"::ls -> let newState = {state with TargetPath = ls.Head}
                                                                                ArgumentParser newState ls.Tail
                                                                  | "-ns"::ls -> let newState = {state with TargetNameSpace = ls.Head}
                                                                                 ArgumentParser newState ls.Tail
                                                                  | _ -> failwith("An error occured at argument parsing")

                                          


[<EntryPoint>]
let main argv =

    let config = ArgumentParser {OriginProjectPath = ""; TargetPath = ""; TargetNameSpace = ""} argv
    
    if not(Directory.Exists(config.TargetPath)) then CommandRunner.RunDotnetProcess(config) |> ignore

    //OriginCrawler.CrawlFileSystemTree config.OriginProjectPath config
    //|> OriginCrawler.CopyProjectFiles config
    let originTree = OriginCrawler.CrawlFileSystemTree config.OriginProjectPath config
    OriginCrawler.CopyProjectFiles config originTree



    0 // return an integer exit code
