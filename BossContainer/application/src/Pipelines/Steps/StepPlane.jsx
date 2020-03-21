import React, { useEffect, useState } from 'react';
import { makeStyles } from '@material-ui/core/styles';
import Step, {StepWidth, StepHeight} from './Step';

const columnMargin = 30;
const pathWidth = 4;

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

function PartitionSteps(steps)
{
    console.log(steps);
    return steps.map((item, i) => {
        return {
            x: (i * 2 + 1) * Math.asin(1)/(steps.length * Math.PI),
            y: 0.5,
            // Add same calculation for internal partition separation
            steps: [{
                    item,
                    paths: item.props.nextSteps.map(x => {
                        return ({
                            x: ((i + 1)* 2 + 1) * Math.asin(1)/(steps.length * Math.PI), // still need to check if we skip past a partition
                            y: 0.5, // find y calc for this later
                            id: x
                        })
                    })
                }]
        }
    })
}

function StepPlane(props)
{
    var partitions = PartitionSteps(props.children);
    var classes = styles();
    var canvasWidth = partitions.length * (StepWidth + (columnMargin * 2));
    var canvasHeight = (partitions.reduce((prev, current) => (prev.steps.length > current.steps.length) ? prev : current).steps.length) * (StepHeight + (columnMargin * 2))
    return (
        <div style={{ width: canvasWidth, height: canvasHeight }} className={classes.plane}>
            <svg width={canvasWidth} height={canvasHeight}>
                <defs>
                    <pattern id="diagonalHatch" patternUnits="userSpaceOnUse" width="8" height="8">
                        <rect x="0" y="0" width="8" height="8" fill="green"/>
                        <rect x="0" y="1" width="8" height="5" fill="white"/>
                        <path d="M-2,2 l4,-4
                                M0,8 l8,-8
                                M6,10 l4,-4" 
                                style={{stroke:"green", strokeWidth:2}} />
                    </pattern>
                </defs>
                {partitions.map((partition, i) => 
                    partition.steps.map((step, j) => {
                        return (
                            <React.Fragment key={step.item.props.id}>
                                <Step {...step.item.props} x={(partition.x * canvasWidth) - (StepWidth / 2)} y={partition.y * canvasHeight - (StepHeight / 2)}/>
                                {step.paths.map(path => {
                                    return <polygon key={`${step.item.props.id}-${path.id}`} fill="url(#diagonalHatch)" points={`${(partition.x * canvasWidth) + (StepWidth / 2)},${partition.y * canvasHeight - pathWidth} ${(path.x * canvasWidth) - (StepWidth / 2)},${path.y * canvasHeight - pathWidth} ${(path.x * canvasWidth) - (StepWidth / 2)},${path.y * canvasHeight + pathWidth} ${(partition.x * canvasWidth) + (StepWidth / 2)},${partition.y * canvasHeight + pathWidth}`}/>
                                })}
                            </React.Fragment>
                        )
                    })
                )}
            </svg>
        </div>
    )
}

export default StepPlane;