import React, { useEffect, useState } from 'react';
import { useParams } from "react-router-dom";

import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import TablePagination from '@material-ui/core/TablePagination';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import Typography from '@material-ui/core/Typography';

import Stop from '@material-ui/icons/Stop';

import Paginate from '../Utilities/Paginate/Paginate';
import { Get } from '../Modules/requests';
import { getHealthSymbol } from '../Modules/helpers';

function ContainerRow(props)
{
    return (
        <TableRow>
            <TableCell>{props.name}</TableCell>
            <TableCell>{getHealthSymbol(props.status.healthy ? "Healthy" : "Unhealthy")}</TableCell>
            <TableCell>{props.status.containerStatus}</TableCell>
            <TableCell><Stop style={{color: "red"}}/></TableCell>
        </TableRow>
    )
}

function Operation()
{
    let [containers, updateContainers] = useState([]);
    var [pageRows, updatePageRows] = useState(10);
    var [currentPage, updateCurrentPage] = useState(0);
    var params = useParams();
    useEffect(() => {
        (async () => {
            var containerList = (await Get(`/api/operations/${params.name}/containers`)).data;
            updateContainers(containerList);
        })();
    }, [])
    return (
        <div>
            <Typography variant="h4">{params.name}</Typography>
            <br/>
            <TableContainer component={"div"}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell>Container</TableCell>
                            <TableCell>Healthy</TableCell>
                            <TableCell>Status</TableCell>
                            <TableCell></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        <Paginate pageCount={pageRows} page={currentPage}>
                            {
                                containers.map(item => {
                                    return <ContainerRow {...item} key={item.name}/>
                                })
                            }
                        </Paginate>
                        <TableRow>
                            <TablePagination 
                                rowsPerPageOptions={[10, 25, 50, 100]}
                                page={currentPage} 
                                rowsPerPage={pageRows} 
                                count={containers.length} 
                                onChangePage={(event, page) => {updateCurrentPage(page)}}
                                onChangeRowsPerPage={(event) => updatePageRows(event.target.value)}
                                />
                        </TableRow>
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    )
}

export default Operation;