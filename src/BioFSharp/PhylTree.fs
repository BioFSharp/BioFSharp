namespace BioFSharp

/// Recursive representation of a phylogenetic tree
type PhylogeneticTree<'T> =
    ///Can be internal node or leaf node, depending on wether the list is empty or not. Match accordingly
    | Branch of 'T * List<PhylogeneticTree<'T>>
    | Leaf of 'T

module PhylogeneticTree =
    ///Iterates trough a tree and transforms all branch and leaf values by applying a mapping function on them
    let map (branchTagMapping: 'T -> 'U) (leafTagMapping: 'T -> 'U) (tree:PhylogeneticTree<'T>) = 
        let rec loop (tree:PhylogeneticTree<'T>) =
            match tree with     
            | Branch (b,children) -> Branch (branchTagMapping b, List.map loop children)
            | Leaf l -> Leaf (l |> leafTagMapping)
        loop tree

    ///Iterates trough a tree and performs an action on every branch and leaf
    let iter (branchAction: 'T -> unit) (leafAction: 'T -> unit) (tree:PhylogeneticTree<'T>) =     
        let rec loop (tree:PhylogeneticTree<'T>) =
            match tree with
            | Branch (b,nl) ->  
                branchAction b
                List.iter loop nl   
            | Leaf l -> leafAction l
        loop tree

    ///Iterates through a tree and accumulates a value by applying the folder to it and every branch and leaf. 
    let fold (folder: 'State -> 'T -> 'State) (acc: 'State) (tree:PhylogeneticTree<'T>) =
        let rec loop (tree:PhylogeneticTree<'T>) (acc:'State) =
            match tree with
            | Branch (b,nl) -> nl |> List.fold (fun acc elem -> loop elem acc) (folder acc b)
            | Leaf l -> folder acc l
        loop tree acc

    /// Returns the count of leaves
    let countLeafs (tree:PhylogeneticTree<'T>) =     
        let mutable leafCount = 0
        tree |> iter (fun _ -> ()) (fun _ -> leafCount <- leafCount+1)
        leafCount

    /// Returns the count of branches
    let countBranches (tree:PhylogeneticTree<'T>) =     
        let mutable branchCount = 0
        tree |> iter (fun _ -> branchCount <- branchCount+1) (fun _ -> ()) 
        branchCount