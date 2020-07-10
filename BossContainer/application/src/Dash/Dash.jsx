import React, { useEffect, useState } from 'react';
import { RadialChart, Hint } from 'react-vis';
import makeStyles from '@material-ui/styles/makeStyles';
import Grid from '@material-ui/core/Grid';

import { Get, Post } from '../Modules/requests';
import { sum } from '../Modules/helpers';
import DashItem from './DashItem';

var styles = makeStyles(theme => ({
    dash: {
        maxWidth: 1024,
        margin: "auto"
    }
}))

function Dash()
{
    var classes = styles();
    var [pipelineData, updatePipelineData] = useState([]);
    var [pipelineSuccess, updatePipelineSuccess] = useState(0);
    var [pipelineFail, updatePipelineFail] = useState(0);
    var [pipelineExecuting, updatePipelineExecuting] = useState(0);
    var [pipelineQueued, updatePipelineQueued] = useState(0);
    var [operations, updateOperations] = useState([]);
    var [operationHealthy, updateOperationHealthy] = useState(0);
    var [operationUnhealthy, updateOperationUnhealthy] = useState(0);
    var [operationPending, updateOperationPending] = useState(0);
    var [pipelineTooltip, updatePipelineTooltip] = useState(null);
    var [operationTooltip, updateOperationTooltip] = useState(null);

    useEffect(didUpdate => {
        (async () => {
            if(!didUpdate)
            {
                // Probably move this into another file at some point
                var pipeNames = (await Get("/api/pipelines", null)).data;
                pipeNames = pipeNames.map(x => { return { ...(x.description), version: x.version } })
                var pipesPromise = pipeNames.map(x => {
                    return {
                        health: Get(`/api/pipelines/${x.name}/health`, null),
                        requests: Get(`/api/pipelines/${x.name}/${x.version}/requests`, null)
                    }
                });
                var pipes = [];
                for(var i = 0; i < pipesPromise.length; i++)
                {
                    console.log(pipesPromise[i].health);
                    pipes.push({
                        ...((await pipesPromise[i].health).data),
                        requests: (await pipesPromise[i].requests).data
                    })
                }
                var operationList = (await Get("/api/operations")).data;
                updateOperations(operationList);
                updatePipelineData(pipes);
                var requests = pipes.map(x => x.requests).flat();
                updatePipelineExecuting(requests.filter(x => x.status === "Executing").length);
                updatePipelineFail(requests.filter(x => x.status === "Failed" || x.status == "Cancelled").length);
                updatePipelineQueued(requests.filter(x => x.status === "Pending").length);
                updatePipelineSuccess(requests.filter(x => x.status === "Complete").length);
                var totalOperations = sum(operationList.map(x => x.targetSize));
                var healthyOperations = sum(operationList.map(x => x.status.healthyContainers));
                var unhealthyOperations = sum(operationList.map(x => x.status.unhealthyContainers));
                updateOperationHealthy(healthyOperations);
                updateOperationUnhealthy(unhealthyOperations);
                updateOperationPending(totalOperations - healthyOperations - unhealthyOperations);
            }
        })();
    }, [])
    var successData = [];
    if(pipelineQueued) successData.push({
        label: "Queued",
        angle: pipelineQueued,
        color: "grey"
    })
    if(pipelineSuccess) successData.push({
        label: "Success",
        angle: pipelineSuccess,
        color: "green"
    })
    if(pipelineExecuting) successData.push({
        label: "Executing",
        angle: pipelineExecuting,
        color: "orange"
    })
    if(pipelineFail) successData.push({
        label: "Failed",
        angle: pipelineFail,
        color: "red"
    })
    var healthData = [];
    if(operationHealthy) healthData.push({
        label: "Healthy",
        angle: operationHealthy,
        color: "green"
    });
    if(operationUnhealthy) healthData.push({
        label: "Unhealthy",
        angle: operationUnhealthy,
        color: "red"
    });
    if(operationPending) healthData.push({
        label: "Pending",
        angle: operationPending,
        color: "grey"
    });
    return (
    <div className={classes.dash}>
        <Grid container spacing={3}>
            <Grid item xs={4}>
                <DashItem
                    title={"Pipeline Success"}
                >
                    <RadialChart
                        colorType="literal"
                        data={successData}
                        width={200}
                        height={200}
                        onValueMouseOver={x => updatePipelineTooltip(x)}
                        onValueMouseOut={x => updatePipelineTooltip(null)}
                    >
                        {pipelineTooltip && 
                        <Hint 
                            value={pipelineTooltip}
                            format={x => { return [{title: x.label, value: Math.round((Math.abs(x.angle - x.angle0) / (2 * Math.PI)) * 100) + "%"}]; }}
                        />}
                    </RadialChart>
                </DashItem>
            </Grid>
            <Grid item xs={4}>
                <DashItem
                    title={"Operation Health"}
                >
                    <RadialChart
                        colorType="literal"
                        data={healthData}
                        width={200}
                        height={200}
                        onValueMouseOver={x => updateOperationTooltip(x)}
                        onValueMouseOut={x => updateOperationTooltip(null)}
                    >
                        {operationTooltip && 
                        <Hint 
                            value={operationTooltip}
                            format={x => { return [{title: x.label, value: Math.round((Math.abs(x.angle - x.angle0) / (2 * Math.PI)) * 100) + "%"}]; }}
                        />}
                    </RadialChart>
                </DashItem>
            </Grid>
        </Grid>
    </div>
    )
}

export default Dash;