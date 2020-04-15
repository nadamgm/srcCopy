// Learn more about F# at http://fsharp.org

open System
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

    let args = ArgumentParser {OriginProjectPath = ""; TargetPath = ""; TargetNameSpace = ""} [|"-t"; "somePath/som"; "-o"; "tragst/sfs/sfs"; "DiffTool"|]
    printfn "%A" args
    printfn "Hello World from F#!"
    0 // return an integer exit code
