import React, { useEffect, useState } from 'react';
import { makeStyles } from '@material-ui/core/styles';
import Step, {StepWidth, StepHeight} from './Step';

const columnMargin = 30;
const pathWidth = 8;

const styles = makeStyles(theme => ({
    stepColumn: {
        margin: `0 ${columnMargin}px 0 ${columnMargin}px`,
        width: StepWidth,
        position: "absolute",
        top: 0
    },
    plane: {
        height: "100%",
        position: "relative"
    }
}));

// This whole thing is pretty inefficient, needs some serious refactor later when my brain doesn't hurt
function PartitionSteps(steps)
{
    var stepMap = {};
    steps.forEach(x => {
        stepMap[x.props.id] = x;
    })
    var partitions = [];
    var partitionMap = {};
    var branchLength = {};
    function getPartition(stepsList)
    {
        var nextSteps = [];
        var branchesFound = [];
        stepsList.forEach(step => {
            if(step.props.isBranch)
            {
                branchLength[step.props.branchEndId] = step.props.nextSteps.length;
            }
            var nextStepsFiltered = [];
            if(!step.placeHolder)
            {
                step.props.nextSteps.forEach(item => {
                    var stepToPush = stepMap[item];
                    if(branchLength[item])
                    {
                        if(branchLength[item] - 1 > 0)
                        {
                            branchLength[item] = branchLength[item] - 1;
                            stepToPush = {
                                props: {},
                                waitingFor: item,
                                placeHolder: true
                            }
                        }
                        else
                        {
                            branchLength[item] = undefined
                            branchesFound.push(item);
                        }
                    }
                    nextStepsFiltered.push(stepToPush)
                })
            }
            else
            {
                nextStepsFiltered.push(step);
            }
            nextSteps = nextSteps.concat(nextStepsFiltered);
            //nextSteps = nextSteps.concat(step.props.nextSteps.map(item => stepMap[item]))
        });
        nextSteps = nextSteps.filter(x => !x.placeHolder || !branchesFound.includes(x.waitingFor)).filter(x => x.placeHolder || !partitionMap[x.props.id]);
        stepsList.forEach(step => {
            partitionMap[step.props.id] = partitions.length
        })
        partitions.push(stepsList);
        if(nextSteps.length > 0)
        {
            getPartition(nextSteps);
        }
    }
    getPartition([steps[0]]);
    console.log(partitions, partitionMap);
    return partitions.map((partition, i) => {
        return {
            x: (i * 2 + 1) * Math.asin(1)/(partitions.length * Math.PI),
            // Add same calculation for internal partition separation
            steps: partition.map((step, j) => {
                if(step.placeHolder) return null;
                return {
                    item: step,
                    y: (j * 2 + 1) * Math.asin(1)/(partition.length * Math.PI),
                    paths: step.props.nextSteps.map((nextStep, k) => {
                        var thisStep = stepMap[nextStep];
                        var forwardDirection = partitionMap[thisStep.props.id] > partitionMap[step.props.id]; 
                        return ({
                            x: (partitionMap[thisStep.props.id] * 2 + 1) * Math.asin(1)/(partitions.length * Math.PI), // still need to check if we skip past a partition
                            y: (partitions[partitionMap[thisStep.props.id]].map(a => a.props.id).indexOf(thisStep.props.id) * 2 + 1) * Math.asin(1)/(partitions[partitionMap[thisStep.props.id]].length * Math.PI), // find y calc for this later
                            id: nextStep,
                            color: thisStep.props.color,
                            skipsPartition: partitionMap[thisStep.props.id] > i + 1,
                            loopBack: !forwardDirection
                        })
                    })
                }
            }).filter(a => a)
        }
    }).filter(x => x);
}

function StepPlane(props)
{
    var partitions = PartitionSteps(props.children);
    var classes = styles();
    var canvasWidth = partitions.length * (StepWidth + (columnMargin * 2));
    var canvasHeight = (partitions.reduce((prev, current) => (prev.steps.length > current.steps.length) ? prev : current).steps.length) * (StepHeight + (columnMargin * 2))
    return (
        <div style={{ width: canvasWidth, height: canvasHeight }} className={classes.plane}>
            <svg width={canvasWidth} height={canvasHeight} xmlns="http://www.w3.org/2000/svg">
                {partitions.map((partition, i) => 
                    partition.steps.map((step, j) => {
                        console.log(partition, step);
                        return (
                            <React.Fragment key={step.item.props.id}>
                                {step.paths.map(path => {
                                    // TODO: this breaks if you have a branch inside a loop
                                    if(path.loopBack)
                                    {
                                        var x1 = (partition.x * canvasWidth);
                                        var y1 = (step.y * canvasHeight) + (StepHeight / 2) - 2;
                                        var x2 = (path.x * canvasWidth);
                                        var y2 = (path.y * canvasHeight)  + (StepHeight / 2) - 2;
                                        return (
                                            <path d={`M ${x1} ${y1} L ${x1} ${y1 + 15} L ${x2} ${y2 + 15} L ${x2} ${y2}`} fill="transparent" stroke={path.color || "grey"} strokeWidth="4" opacity="0.55"/>
                                        )
                                    }
                                    else
                                    {
                                        var x1 = (partition.x * canvasWidth) + (StepWidth / 2) - 2;
                                        var y1 = (step.y * canvasHeight);
                                        var x2 = (path.x * canvasWidth) - (StepWidth / 2) + 2;
                                        var y2 = (path.y * canvasHeight);
                                        var length = Math.sqrt(Math.pow((x2 - x1), 2) + Math.pow((y2 - y1), 2));
                                        var angle = Math.atan((y2 - y1) / (x2 - x1)) * (180 / Math.PI);
                                        return (
                                            <path d={path.skipsPartition ? 
                                                `M ${x1} ${y1} Q ${x2} ${y1}, ${x2} ${y2}`
                                                :
                                                `M ${x1} ${y1} L ${x2} ${y2}`    
                                            } fill="transparent" stroke={path.color || "grey"} strokeWidth="4" opacity="0.55"/>
                                        )
                                    }
                                })}
                                <Step {...step.item.props} x={(partition.x * canvasWidth) - (StepWidth / 2)} y={step.y * canvasHeight - (StepHeight / 2)}/>
                            </React.Fragment>
                        )
                    })
                )}
            </svg>
        </div>
    )
}

export default StepPlane;