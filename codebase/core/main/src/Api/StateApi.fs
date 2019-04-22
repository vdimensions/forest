//
// Copyright 2014-2019 vdimensions.net.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
namespace Forest

/// An interface representing a forest state visitor
type [<Interface>] internal IForestStateVisitor =
    /// Called upon visiting a sibling or child BFS-style
    abstract member BFS: node : TreeNode -> index : int -> model : obj -> descriptor : IViewDescriptor -> unit
    /// Called upon visiting a sibling or child DFS-style
    abstract member DFS: node : TreeNode -> index : int -> model : obj -> descriptor : IViewDescriptor -> unit
    /// Executed once when the traversal is complete.
    abstract member Complete: unit -> unit