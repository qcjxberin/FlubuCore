using System;
using System.Collections.Generic;
using Flubu.Context;
using Flubu.Tasks;

namespace Flubu.Targeting
{
    public class Target : TaskBase, ITarget
    {
        private readonly List<string> _dependencies = new List<string>();

        private readonly List<ITask> _tasks = new List<ITask>();

        private string _description;

        private Action<ITaskContext> _targetAction;

        private TargetTree _targetTree;

        public Target(string targetName)
        {
            TargetName = targetName;
        }

        public Target(TargetTree targetTree, string targetName)
        {
            _targetTree = targetTree;
            TargetName = targetName;
        }

        public ICollection<string> Dependencies => _dependencies;

        /// <summary>
        ///     Gets the description of the target.
        /// </summary>
        /// <value>The description of the target.</value>
        public override string Description => _description;

        /// <summary>
        ///     Gets a value indicating whether this target is hidden. Hidden targets will not be
        ///     visible in the list of targets displayed to the user as help.
        /// </summary>
        /// <value><c>true</c> if this target is hidden; otherwise, <c>false</c>.</value>
        public bool IsHidden { get; private set; }

        public string TargetName { get; }

        protected override bool LogDuration => true;

        protected override string DescriptionForLog => TargetName;

        public static ITarget Create(string targetName)
        {
            return new Target(targetName);
        }

        /// <summary>
        ///     Specifies targets on which this target depends on.
        /// </summary>
        /// <param name="targetNames">The dependency target names.</param>
        /// <returns>This same instance of <see cref="ITarget" />.</returns>
        public ITarget DependsOn(params string[] targetNames)
        {
            foreach (var dependentTargetName in targetNames)
            {
                _dependencies.Add(dependentTargetName);
            }

            return this;
        }

        public ITarget Do(Action<ITaskContext> targetAction)
        {
            if (_targetAction != null)
            {
                throw new ArgumentException("Target action was already set.");
            }

            _targetAction = targetAction;
            return this;
        }

        public ITarget OverrideDo(Action<ITaskContext> targetAction)
        {
            _targetAction = targetAction;
            return this;
        }

        /// <summary>
        ///     Sets the target as the default target for the runner.
        /// </summary>
        /// <returns>This same instance of <see cref="ITarget" />.</returns>
        public ITarget SetAsDefault()
        {
            _targetTree.SetDefaultTarget(this);
            return this;
        }

        /// <summary>
        ///     Set's the description of the target,
        /// </summary>
        /// <param name="description">The description</param>
        /// <returns>this target</returns>
        public ITarget SetDescription(string description)
        {
            _description = description;
            return this;
        }

        /// <summary>
        ///     Sets the target as hidden. Hidden targets will not be
        ///     visible in the list of targets displayed to the user as help.
        /// </summary>
        /// <returns>This same instance of <see cref="ITarget" />.</returns>
        public ITarget SetAsHidden()
        {
            IsHidden = true;
            return this;
        }

        /// <summary>
        ///     Adds this target to target tree.
        /// </summary>
        /// <param name="targetTree">The <see cref="TargetTree" /> that target will be added to.</param>
        public TargetTree AddToTargetTree(TargetTree targetTree)
        {
            _targetTree = targetTree;
            targetTree.AddTarget(this);
            return targetTree;
        }

        /// <summary>
        ///     Specifies targets on which this target depends on.
        /// </summary>
        /// <param name="targets">The dependency targets</param>
        /// <returns>This same instance of <see cref="ITarget" /></returns>
        public ITarget DependsOn(params ITarget[] targets)
        {
            foreach (var target in targets)
            {
                _dependencies.Add(target.TargetName);
            }

            return this;
        }

        public ITarget AddTask(params ITask[] tasks)
        {
            _tasks.AddRange(tasks);
            return this;
        }

        protected override int DoExecute(ITaskContext context)
        {
            if (_targetTree == null)
            {
                throw new ArgumentNullException(nameof(_targetTree), "TargetTree must be set before Execution of target.");
            }

            _targetTree.MarkTargetAsExecuted(this);
            _targetTree.EnsureDependenciesExecuted(context, TargetName);

            // we can have action-less targets (that only depend on other targets)
            _targetAction?.Invoke(context);

            int res = 0;

            foreach (ITask task in _tasks)
            {
                res = task.Execute(context);
            }

            return res;
        }
    }
}